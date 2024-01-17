using nyzoNode.Alignment.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nyzoNode.Alignment.Core.Client.Commands;

internal class ExitCommand {
	public string ShortCommand => "X";

	public string LongCommand => "exit";

	public string Description => "exit Nyzo client";

	public IList<string> ArgumentNames { get; init; } = new List<string>();

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>();

	public bool RequiresValidation => false;

	public bool RequiresConfirmation => false;

	public bool IsLongRunning => false;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		string message = "Thank you for using the Nyzo client!";
		
		ConsoleUtil.PrintTable(
			new List<List<string>> {
				new List<string> {
					message
				}
			}
		);

		UpdateUtil.Terminate();

		return null!;
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		return null!;
	}
}
