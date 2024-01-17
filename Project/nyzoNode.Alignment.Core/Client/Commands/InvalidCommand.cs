using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nyzoNode.Alignment.Core.Client.Commands; 
internal class InvalidCommand : ICommand {
	public string ShortCommand => "I";

	public string LongCommand => "invalid";

	public string Description => "an invalid message was provided";

	public IList<string> ArgumentNames { get; init; } = new List<string>();

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>();

	public bool RequiresValidation => false;

	public bool RequiresConfirmation => false;

	public bool IsLongRunning => false;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		CommandManager.PrintCommands(commandOutput);

		// todo color
		commandOutput.PrintLines(
			new List<string>() {
				$"{Color.Red} Your selection was not recognized.",
				$"{Color.Red} Please choose an option from the above commands.",
				$"{Color.Red} You may type either the short command or the full command."
			}	
		);

		return null!;
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		return null!;
	}
}
