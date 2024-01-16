using nyzoNode.Alignment.Core.NyzoString;
using nyzoNode.Alignment.Core.Util;
using System.Text.RegularExpressions;

namespace nyzoNode.Alignment.Core.Client.Commands;

internal class BalanceDisplayCommand : ICommand {
	private const int _maximumAccountsInResponse = 100;

	public string ShortCommand => "BL";

	public string LongCommand => "balance";

	public string Description => "display wallet balances";

	public IList<string> ArgumentNames { get; init; } = new List<string>() {
		"wallet ID or prefix"
	};

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>() {
		"walletId"
	};

	public bool RequiresValidation => false;

	public bool RequiresConfirmation => false;

	public bool IsLongRunning => false;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		string walletIdOrPrefix = argumentValues.Count < 1 ? "" : argumentValues[0];
		walletIdOrPrefix = walletIdOrPrefix?.Trim()?.ToLower() ?? "";

		// Normalize for a raw ID
		string rawIdOrPrefix = Regex.Replace(walletIdOrPrefix, "[^a-f0-9]", "");

		// Normalize for a Nyzo string, this is correct behavior, while Nyzo strings are case-sensitive, the search is designed to be case-insensitive. In the loop through balance-list items below, the generated Nyzo strings are also converted to lowercase
		string nyzoStringIdOrPrefix = walletIdOrPrefix.ToLower();

		// Make the lists for the notices and errors & make the result table
		var notices = new List<string>();
		var errors = new List<string>();
		CommandTable commandTable = new CommandTable(
			new CommandTableHeader("block height", "blockHeight"),
			new CommandTableHeader("wallet ID", "walletId"),
			new CommandTableHeader("ID string", "walletIdNyzoString"),
			new CommandTableHeader("balance", "balance")
		);

		// Determine whether the search is performed with a Nyzo string
		bool searchIsNyzoString = nyzoStringIdOrPrefix.StartsWith("id__");

		// Add a notice showing the prefix after normalization & what type of search is being performed
		notices.AddRange(new List<string>() {
			$"wallet ID or prefix after normalization {(searchIsNyzoString ? nyzoStringIdOrPrefix : rawIdOrPrefix)}",
			$"search type: {(searchIsNyzoString ? "Nyzo string" : "raw ID")}"
		});

		// Produce the results
		if (!walletIdOrPrefix.Any()) {
			errors.Add("You must provide a wallet ID or prefix");
		}

		if (!errors.Any()) {
			BalanceList balanceList = BalanceListManager.GetFrozenEdgeList();

			if(balanceList is null) {
				errors.Add("Unable to get balance list");
			}

			if (!errors.Any()) {
				int numberFound = 0;

				List<BalanceListItem> balanceListItems = balanceList.Items;

				for(int i = 0; i < balanceListItems.Count && numberFound < _maximumAccountsInResponse; i++) {
					BalanceListItem balanceListItem = balanceListItems[i];

					string identifier = ByteUtil.ArrayAsStringNoDashes(balanceListItem.Identifier);
					NyzoStringPublicIdentifier nyzoStringPublicIdentifier = new(balanceListItem.Identifier);

					string identifierString = NyzoStringEncoder.Encode(nyzoStringPublicIdentifier).toLower();

					if((searchIsNyzoString && identifier.StartsWith(rawIdOrPrefix)) || (!searchIsNyzoString && identifier.StartsWith(rawIdOrPrefix))) {
						numberFound++;
						commandTable.AddRow(
							balanceList.BlockHeight,
							identifier,
							NyzoStringEncoder.Encode(nyzoStringPublicIdentifier), // correct, mixed-case string
							PrintUtil.PrintAmount(balanceListItem.Balance)
						);
					}
				}

				if(numberFound == 0) {
					notices.Add($"unable to find any accounts matching ID/prefix {(searchIsNyzoString ? nyzoStringIdOrPrefix : rawIdOrPrefix)}");
				}

				if(numberFound == _maximumAccountsInResponse) {
					notices.Add($"search is returning a maximum of {_maximumAccountsInResponse} results, more result may be available");
				}
			}
		}

		return new SimpleExecutionResult(notices, errors, commandTable);
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		return null!;
	}
}
