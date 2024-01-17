using nyzoNode.Alignment.Core.NyzoScript;
using nyzoNode.Alignment.Core.NyzoString;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nyzoNode.Alignment.Core.Client.Commands;

internal class NyzoScriptCommand : ICommand {
	public string ShortCommand => "NS";

	public string LongCommand => "script";

	public string Description => "get the state of a Nyzo script";

	public IList<string> ArgumentNames { get; init; } = new List<string>() {
		"account ID", "include unconfirmed transactions"
	};

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>() {
		"accountIdentifier", "includeUnconfirmedTransactions"
	};

	public bool RequiresValidation => false;

	public bool RequiresConfirmation => false;

	public bool IsLongRunning => false;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		var notices = new List<string>();
		var errors = new List<string>();

		// Get the account identifier
		NyzoStringPublicIdentifier accountIdentifier = ClientArgumentUtil.ParsePublicIdentifier(argumentValues[0]);
		bool includeUnconfirmedTransactions = ClientArgumentUtil.ParseBoolean(argumentValues[1], false);

		// Add raw account identifier & Nyzo string to notices
		notices.AddRange(
			new List<string>() {
				$"Account identifier (raw): {ByteUtil.ArrayAsStringWithDashes(accountIdentifier.Bytes)}",
				$"Account identifier (Nyzo string): {NyzoStringEncoder.Encode(accountIdentifier)}"
			}	
		);

		// Produce result using current state
		NyzoScriptState nyzoScriptState = NyzoScriptManager.ProvideStateForAccount(
			new ByteBuffer(accountIdentifier.Bytes),
			includeUnconfirmedTransactions
		);

		if(nyzoScriptState is null) {
			errors.Add(
				$"Unable to get state for account {argumentValues[0]} ({ByteUtil.ArrayAsStringWithDashes(accountIdentifier.Bytes)})"
			);
		}

		var commandTable = new CommandTable(
			new CommandTableHeader("creation height", "creationHeight"),	
			new CommandTableHeader("last update height", "lastUpdateHeight"),	
			new CommandTableHeader("frozen edge height", "frozenEdgeHeight"),	
			new CommandTableHeader("content type", "contentType"),	
			new CommandTableHeader("contains unconfirmed data", "containsUnconfirmedData")
		);

		commandTable.SetInvertedRowsAndColumns(true);

		if(nyzoScriptState is null) {
			commandTable.AddRow("-", "-", "-", "-", "-");
		}

		if(!commandTable.Rows.Any()) {
			commandTable.AddRow(
				nyzoScriptState.CreationHeight,
				nyzoScriptState.LastUpdateHeight,
				BlockManager.FrozenEdgeHeight,
				nyzoScriptState.ContentType,
				nyzoScriptState.ContainsUnconfirmedData
			);
		}

		return new NyzoScriptSimpleExecutionResult(notices, errors, commandTable, nyzoScriptState, this);
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		return null!;
	}
}
