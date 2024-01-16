using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nyzoNode.Alignment.Core.Client.Commands; 
internal interface ICommand {
	string ShortCommand { get; }
	string LongCommand { get; }
	string Description { get; }
	IList<string> ArgumentNames { get; }
	IList<string> ArgumentIdentifiers { get; }
	bool RequiresValidation { get; }
	bool RequiresConfirmation { get; }
	bool IsLongRunning { get; }
	ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput);
	ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput);
}
