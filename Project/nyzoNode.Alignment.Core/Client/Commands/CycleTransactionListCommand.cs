using nyzoNode.Alignment.Core.NyzoString;
using nyzoNode.Alignment.Core.Util;
using nyzoNode.Alignment.Core.Web;
using System.Drawing;

namespace nyzoNode.Alignment.Core.Client.Commands;
internal class CycleTransactionListCommand : ICommand {
	public string ShortCommand => "CTL";

	public string LongCommand => "cycleList";

	public string Description => "list cycle transactions";

	public IList<string> ArgumentNames { get; init; } = new List<string>();

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>();

	public bool RequiresValidation => false;

	public bool RequiresConfirmation => false;

	public bool IsLongRunning => false;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		// Prepare lists for result
		var notices = new List<string>();
		var errors = new List<string>();
		var commandTable = new CommandTable(
			new CommandTableHeader("initiator ID", "initiatorId", true),
			new CommandTableHeader("initiator ID string", "initiatorIdNyzoString", true),
			new CommandTableHeader("receiver ID", "receiverId", true),
			new CommandTableHeader("receiver ID string", "receiverIdNyzoString", true),
			new CommandTableHeader("amount", "amount"),
			new CommandTableHeader("height", "height"),
			new CommandTableHeader("initiator data", "initiatorData", true),
			new CommandTableHeader("amt of votes", "numberOfVotes"),
			new CommandTableHeader("amt of yes votes", "numberOfYesVotes"),
			new CommandTableHeader("signature", "signature", true),
			new CommandTableHeader("signature string", "signatureNyzoString", true)
		);

		try {
			// Get the balance list
			var balanceList = BalanceListManager.FrozenEdgeList;

			if(balanceList is null) {
				errors.Add("balance list is null");
			} else if (balanceList.PendingCycleTransactions.Count == 0) {
				notices.Add("no cycle transactions in balance list");
			}

			if(!errors.Any() && !notices.Any()) {
				var distanceFromOpen = BlockManager.OpenEdgeHeight(false) - balanceList.BlockHeight;

				notices.Add($"using balance list at height {balanceList.BlockHeight}, {distanceFromOpen} from open edge");

				foreach(Transaction transaction in balanceList.PendingCycleTransactions.Values) {
					var row = new List<object>();

					// Add sender identifier columns
					byte[] senderIdentifier = transaction.SenderIdentifier;
					row.Add(ByteUtil.ArrayAsStringWithDashes(senderIdentifier));
					row.Add(NyzoStringEncoder.Encode(
						new NyzoStringPublicIdentifier(senderIdentifier)
					));

					// Add receiver identifier columns
					byte[] receiverIdentifier = transaction.ReceiverIdentifier();
					row.Add(ByteUtil.ArrayAsStringWithDashes(receiverIdentifier));
					row.Add(NyzoStringEncoder.Encode(
						new NyzoStringPublicIdentifier(receiverIdentifier)
					));

					// Add the amount, height, initiatore data, number of cycle signatures, and number of "yes" votes
					row.Add(PrintUtil.PrintAmount(transaction.Amount));
					row.Add(BlockManager.HeightForTimestamp(transaction.Timestamp));
					row.Add(WebUtil.SanitizedSenderDataForDisplay(transaction.SenderData));
					row.Add(transaction.CycleSignatureTransactions.Count());
					row.Add(this.ProvideAmountOfYesVotes(transaction.CycleSignatureTransactions.Values));

					// Add signature columns
					row.Add(ByteUtil.ArrayAsStringWithDashes(transaction.Signature));
					row.Add(NyzoStringEncoder.Encode(
						new NyzoStringSignature(transaction.Signature)	
					));

					// Add row to table
					commandTable.AddRow(row); // todo assert if ToArray is necessary, might as well overload the fn
				}
			}
		} catch (Exception e) {
			// todo
			// output.println(ConsoleColor.Red + "unexpected issue listing cycle transactions: " + PrintUtil.printException(e) + ConsoleColor.reset);
			commandOutput.PrintLine($"{Color.Red} unexpected issue listing cycle transactions: {PrintUtil.PrintException(e)}"); 
		}


		return new SimpleExecutionResult(notices, errors, commandTable);
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		return null!;
	}

	private static int ProvideAmountOfYesVotes(List<Transaction> signatureTransactions) {
		var amountOfYesVotes = 0;

		foreach(Transaction tx in signatureTransactions) {
			if(tx.CycleTransactionVote == 1) {
				amountOfYesVotes++;
			}
		}

		return amountOfYesVotes;
	}
}
