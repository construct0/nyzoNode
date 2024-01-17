namespace nyzoNode.Alignment.Core.Client.Commands;

internal class FrozenEdgeCommand : ICommand {
	public string ShortCommand => "FE";

	public string LongCommand => "frozenEdge";

	public string Description => "display the frozen edge";

	public IList<string> ArgumentNames { get; init; } = new List<string>();

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>();

	public bool RequiresValidation => false;

	public bool RequiresConfirmation => false;

	public bool IsLongRunning => false;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		// Make result constituents
		var notices = new List<string>();
		var errors = new List<string>();
		var commandTable = new CommandTable(
			new CommandTableHeader("height", "height"),
			new CommandTableHeader("hash", "hash", true),
			new CommandTableHeader("verification timestamp (ms)", "verificationTimestampMilliseconds", false),
			new CommandTableHeader("distance from open edge", "distanceFromOpenEdge"),
			new CommandTableHeader("clientVersion", "clientVersion")
		);

		commandTable.SetInvertedRowsAndColumns(true);

		// Get the block & balance list
		Block? block = BlockManager.FrozenEdge;

		// Add data to command table
		if(block is null) {
			errors.Add("Unable to get frozen edge");
		}

		if (!errors.Any()) {
			Version version = new Version();

			commandTable.AddRow(
				block.BlockHeight,
				ByteUtil.ArrayAsStringWithDashes(block.Hash),
				block.VerificationTimestamp,
				(int)(BlockManager.OpenEdgeHeight(false) - block.BlockHeight),
				$"{version.Version}.{version.SubVersion}"
			);
		}

		return new SimpleExecutionResult(notices, errors, commandTable);
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		return null!;
	}
}
