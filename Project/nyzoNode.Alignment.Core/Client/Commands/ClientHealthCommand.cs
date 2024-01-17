namespace nyzoNode.Alignment.Core.Client.Commands;

internal class ClientHealthCommand : ICommand {
	public string ShortCommand => "HTH";

	public string LongCommand => "health";

	public string Description => "display health metrics";

	public IList<string> ArgumentNames { get; init; } = new List<string>();

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>();

	public bool RequiresValidation => false;

	public bool RequiresConfirmation => false;

	public bool IsLongRunning => false;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		// Make the lists for the notices and errors & make the result command table
		var notices = new List<string>();
		var errors = new List<string>();
		CommandTable commandTable = new(
			new CommandTableHeader("amt of nodes in mesh", "numberNodesInMesh"),
			new CommandTableHeader("amt of preferred nodes", "numberOfPreferredNodesInMesh"),
			new CommandTableHeader("amt of non-preferred fetches", "numberOfNonPreferredFetches"),
			new CommandTableHeader("amt of preferred fetches", "numberOfPreferredFetches"),
			new CommandTableHeader("amt of successful block fetches", "numberOfSuccessfulBlockFetches"),
			new CommandTableHeader("amt of consecutive successful block fetches", "numberOfConsecutiveSuccessfulBlockFetches"),
			new CommandTableHeader("amt of unsuccessful block fetches", "numberOfUnsuccessfulBlockFetches"),
			new CommandTableHeader("amt of consecutive unsuccessful block fetches", "numberOfConsecutiveUnsuccessfulBlockFetches"),
			new CommandTableHeader("frozen edge verification age (ms)", "frozenEdgeVerificationAgeMilliseconds")
		);

		commandTable.setInvertedRowsAndColumns(true);

		// Add results to command table
		Block frozenEdge = BlockManager.FrozenEdge;
		commandTable.AddRow(
			ClientNodeManager.NumberOfNodesInMesh,
			ClientNodeManager.NumberOfPreferredNodesInMesh,
			ClientNodeManager.numberOfNonPreferredFetches,
			ClientNodeManager.numberOfPreferredFetches,
			ClientNodeManager.numberOfSuccessfulBlockFetches,
			ClientNodeManager.numberOfConsecutiveSuccessfulBlockFetches,
			ClientNodeManager.numberOfUnsuccessfulBlockFetches,
			ClientNodeManager.numberOfConsecutiveUnsuccessfulBlockFetches,
			frozenEdge is null 
				? Double.NaN 
				: (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - frozenEdge.VerificationTimestamp)
		);

		// Frozen edge error
		if(frozenEdge is null) {
			errors.Add("Unable to get frozen edge");
		}

		return new SimpleExecutionResult(notices, errors, commandTable);
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		return null!;
	}
}
