using nyzoNode.Alignment.Core.Util;

namespace nyzoNode.Alignment.Core.Client.Commands;
internal class CoinsInCirculationCommand : ICommand {
	public string ShortCommand => "CC";

	public string LongCommand => "circulation";

	public string Description => "display coins in circulation";

	public IList<string> ArgumentNames { get; init; } = new List<string>();

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>();

	public bool RequiresValidation => false;

	public bool RequiresConfirmation => false;

	public bool IsLongRunning => false;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		// Prepare result constituents
		var notices = new List<string>();
		var errors = new List<string>();
		var commandTable = new CommandTable(
			new CommandTableHeader("block height", "blockHeight"),
			new CommandTableHeader("coins in system", "coinsInSystem"),
			new CommandTableHeader("locked account sum", "lockedAccountSum"),
			new CommandTableHeader("unlock threshold", "unlockThreshold"),
			new CommandTableHeader("unlock transfer sum", "unlockTransferSum"),
			new CommandTableHeader("seed account balance", "seedAccountBalance"),
			new CommandTableHeader("transfer account balance", "transferAccountBalance"),
			new CommandTableHeader("cycle account balance", "cycleAccountBalance"),
			new CommandTableHeader("coins in circulation", "coinsInCirculation")
		);

		commandTable.SetInvertedRowsAndColumns(true);

		// Prepare variable
		long totalCirculation = 0;

		// Get the frozen edge balance list & continue if it is available
		var balanceList = BalanceListManager.FrozenEdgeList;

		if(balanceList is null) {
			errors.Add("No balance lists are available on this system");
		}

		if (!errors.Any()) {
			// Make a map of the balances on the balance list
			Dictionary<ByteBuffer, long> balances = BalanceManager.MakeBalanceDict(balanceList);

			// Calculate sum in locked accounts
			long sumInLockedAccounts = 0;

			foreach(ByteBuffer identifier in balances.Keys) {
				if (LockedAccountManager.AccountIsLocked(identifier)) {
					sumInLockedAccounts += balances[identifier];
				}
			}

			// Calculate how much of the coins in the locked | locking accounts has been unlocked
			long unlockedAmountInLockedAccounts = Math.Min(sumInLockedAccounts, balanceList.UnlockThreshold - balanceList.UnlockTransferSum);
			long lockedAmountInLockedAccounts = sumInLockedAccounts - unlockedAmountInLockedAccounts;

			// Get balances of other accounts which are not in circulation
			var byteBufferSeedAccountIdentifier = new ByteBuffer(BalanceManager.SeedAccountIdentifier);
			var byteBufferTransferIdentifier = new ByteBuffer(BalanceListItem.TransferIdentifier);
			var byteBufferCycleAccountBalance = new ByteBuffer(BalanceListItem.CycleAccountIdentifier);

			long seedAccountBalance = balances.TryGetByteBufferValue(byteBufferSeedAccountIdentifier);
			long transferAccountBalance = balances.TryGetByteBufferValue(byteBufferTransferIdentifier);
			long cycleAccountBalance = balances.TryGetByteBufferValue(byteBufferCycleAccountBalance);

			// Calculate total circulation
			totalCirculation = Transaction.MicroNyzosInSystem - lockedAmountInLockedAccounts - seedAccountBalance - transferAccountBalance - cycleAccountBalance;

			// Add cells showing how each line affect the number of coins in circulation
			commandTable.AddRow("", "+", "-", "+", "-", "-", "-", "-", "=");

			// Add data to the table
			commandTable.AddRow(
				balanceList.BlockHeight,
				PrintUtil.PrintAmountWithCommas(Transaction.MicroNyzosInSystem),
				PrintUtil.PrintAmountWithCommas(sumInLockedAccounts),
				PrintUtil.PrintAmountWithCommas(balanceList.UnlockThreshold),
				PrintUtil.PrintAmountWithCommas(balanceList.UnlockTransferSum),
				PrintUtil.PrintAmountWithCommas(seedAccountBalance),
				PrintUtil.PrintAmountWithCommas(transferAccountBalance),
				PrintUtil.PrintAmountWithCommas(cycleAccountBalance),
				PrintUtil.PrintAmountWithCommas(totalCirculation)
			);
		}

		return new CoinsInCirculationSimpleExecutionResult(notices, errors, commandTable, totalCirculation);
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		return null!;
	}
}
