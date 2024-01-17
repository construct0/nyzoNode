using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nyzoNode.Alignment.Core.Client.Commands;

internal class EmptyCommand : ICommand {
	public string ShortCommand => "";

	public string LongCommand => "empty";

	public string Description => "placeholder command handler used when an empty command is provided | received";

	public IList<string> ArgumentNames { get; init; } = new List<string>() {
		""
	};

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>() {
		""
	};

	public bool RequiresValidation => false;

	public bool RequiresConfirmation => false;

	public bool IsLongRunning => false;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		CommandManager.PrintCommands(commandOutput));
		return null!;
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		return null!;
	}
}
