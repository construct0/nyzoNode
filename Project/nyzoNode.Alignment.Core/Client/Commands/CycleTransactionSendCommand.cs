using nyzoNode.Alignment.Core.NyzoString;
using nyzoNode.Alignment.Core.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nyzoNode.Alignment.Core.Client.Commands; 

internal class CycleTransactionSendCommand : ICommand {
	public string ShortCommand => "CTS";

	public string LongCommand => "cycleSend";

	public string Description => "send a cycle transaction";

	public IList<string> ArgumentNames { get; init; } = new List<string>() {
		"initiator key (in-cycle verifier)", "receiver ID", "sender data", "amt of Nyzos"
	};

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>() {
		"initiatorKey", "receiverId", "senderData", "amount"
	};

	public bool RequiresValidation => true;

	public bool RequiresConfirmation => true;

	public bool IsLongRunning => true;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		try {
			// Get the arguments
			// todo cont.

		} catch (Exception e) {
			/*output.println(ConsoleColor.Red + "unexpected issue creating cycle transaction: " +
                    PrintUtil.printException(e) + ConsoleColor.reset);*/
			commandOutput.PrintLine($"{Color.Red} unexpected issue creating cycle transaction: {PrintUtil.PrintException(e)}"); // todo colors 
		}


		return new SimpleExecutionResult(notices, errors, commandTable);
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		ValidationResult result = null!;

		try {
			// Make a list for the argument result items
			var argumentResults = new List<ArgumentResult>();

			// Check initiator key
			NyzoString.NyzoString initiatorKey = NyzoStringEncoder.Decode(
				argumentValues[0]
			);

			if(initiatorKey.GetType() == typeof(NyzoStringPrivateSeed))) {
				argumentResults.Add(
					new ArgumentResult(true, NyzoStringEncoder.Encode(initiatorKey))
				);
			}

			if(!argumentResults.Any()) {
				string message = String.IsNullOrWhiteSpace(argumentValues[0]?.Trim()?.ToString() ?? "") ? "missing Nyzo string private key" : "not a valid Nyzo string private key";
				argumentResults.Add(
					new ArgumentResult(false, argumentValues[0], message)
				);
			}

			// Check the receiver ID
			NyzoString.NyzoString receiverIdentifier = NyzoStringEncoder.Decode(argumentValues[1]);

			if(receiverIdentifier.GetType() == typeof(NyzoStringPublicIdentifier)) {
				argumentResults.Add(
					new ArgumentResult(true, NyzoStringEncoder.Encode(receiverIdentifier))	
				);
			} else {
				string message = String.IsNullOrWhiteSpace(argumentValues[1]?.Trim()?.ToString() ?? "") ? "missing Nyzo string public ID" : "not a valid Nyzo string public ID";
				argumentResults.Add(
					new ArgumentResult(false, argumentValues[0], message)	
				);

				if (argumentValues[0].Length >= 64) {
					PublicNyzoStringCommand.PrintHexWarning(commandOutput);
				}
			}

			// Process the sender data
			string senderDataInputString = argumentValues[2];
			byte[] senderDataBytes;
			string senderDataMessage = String.Empty;
			string senderData = null!;

			// This is the case for raw bytes of sender data 
			if (ClientTransactionUtil.IsNormalizedSenderDataString(senderDataInputString)) {
				senderDataBytes = ClientTransactionUtil.BytesFromNormalizedSenderDataString(senderDataInputString));
				senderDataMessage = "raw bytes";
				senderData = ClientTransactionUtil.NormalizedSenderDataStringFromBytes(senderDataBytes);
			} else {
				// This is the case for plain text sender data
				senderDataBytes = Encoding.UTF8.GetBytes(senderDataInputString);

				if(senderDataBytes.Length > FieldByteSize.MaximumSenderDataLength) {
					senderDataMessage = $"sender data is too long, truncating";
					senderDataBytes = Array.Resize(senderDataBytes, FieldByteSize.MaximumSenderDataLength); // todo test
				}

				senderData = Encoding.UTF8.GetString(senderDataBytes);
			}

			argumentResults.Add(
				new ArgumentResult(true, senderData, senderDataMessage)
			);

			// Check the amount
			long amountOfMicroNyzos = -1;

			try {
				amountOfMicroNyzos = (long)(Double.Parse(argumentValues[3]) * Transaction.MicroNyzoMultiplierRatio);
			} catch (Exception e) {
				LogUtil.PrintLine($"Failed to parse amount of microNyzos from {argumentValues[3]?.ToString() ?? ""}");
			}

			if(amountOfMicroNyzos > 0) {
				double amountOfNyzos = amountOfMicroNyzos / (double)Transaction.MicroNyzoMultiplierRatio;
				argumentResults.Add(
					new ArgumentResult(true, string.Format("{0:0.######}", amountOfNyzos), "")
				);
			} else {
				argumentResults.Add(
					new ArgumentResult(false, argumentValues[3], "invalid amount")	
				);
			}

			result = new ValidationResult(argumentResults);

		} catch(Exception e) {
			LogUtil.PrintLine($"Failed to process {nameof(CycleTransactionSendCommand)} - args: {argumentValues}");
		}

		// If the confirmation result is null, create exception result
		// This happens exclusively when an exception is not handled properly by the validation code
		if(result is null) {
			// todo sufficient? or other reflection method
			result = ValidationResult.FromExceptionResult(MethodInfo.GetCurrentMethod()?.ToString() + $", args: {argumentValues}");
		}

		return result;
	}


}
