using nyzoNode.Alignment.Core.NyzoString;
using nyzoNode.Alignment.Core.Util;
using System.Drawing;
using System.Reflection;

namespace nyzoNode.Alignment.Core.Client.Commands;

internal class CycleTransactionSignCommand : ICommand {
	public string ShortCommand => "CTX";

	public string LongCommand => "cycleSign";

	public string Description => "sign a cycle transaction";

	public IList<string> ArgumentNames { get; init; } = new List<string>() {
		"transaction signature", "signer key", "vote (1=yes, 0=no)"
	};

	public IList<string> ArgumentIdentifiers { get; init; } = new List<string>() {
		"transactionSignature", "signerKey", "vote"
	};

	public bool RequiresValidation => true;

	public bool RequiresConfirmation => true;

	public bool IsLongRunning => true;

	public ExecutionResult Run(IList<string> argumentValues, CommandOutput commandOutput) {
		try {
			// Get the arguments
			NyzoStringSignature transactionSignature = (NyzoStringSignature)NyzoStringEncoder.Decode(argumentValues[0]);
			NyzoStringPrivateSeed signerSeed = (NyzoStringPrivateSeed)NyzoStringEncoder.Decode(argumentValues[1]);
			byte vote = ((argumentValues[2] ?? "") == "1") ? (byte)1 : (byte)0;

			// Create signature transaction
			long timestamp = ClientTransactionUtil.SuggestedTransactionTimestamp;
			Transaction transaction = Transaction.GetCycleSignatureTransaction(timestamp, vote, transactionSignature.Signature, signerSeed.Seed);

			// Send transaction to verifiers
			ClientTransactionUtil.SendTransactionToLikelyBlockVerifiers(transaction, true, commandOutput);

		} catch(Exception e) {
			// todo
			commandOutput.PrintLine($"{Color.Red} unexpected issue sending cycle-signature transaction: {PrintUtil.PrintException(e)}");
		}

		// ExecutionResult objects are not yet implemented for long running commands [todo]
		return null!;
	}

	public ValidationResult Validate(IList<string> argumentValues, CommandOutput commandOutput) {
		ValidationResult result = null!;

		try {
			// Make a list for the argument result items
			var argumentResults = new List<ArgumentResult>();

			// Check transaction signature
			NyzoString.NyzoString transactionSignature = NyzoStringEncoder.Decode(argumentValues[0]);

			if(transactionSignature.GetType().IsInstanceOfType(typeof(NyzoStringSignature))) {
				argumentResults.Add(
					new ArgumentResult(true, NyzoStringEncoder.Encode(transactionSignature))
				);
			}

			if(!argumentResults.Any()) {
				string message = String.IsNullOrEmpty(argumentValues[0]?.Trim() ?? "") ? "missing Nyzo string transaction signature" : "not a valid Nyzo string transaction signature";
				argumentResults.Add(
					new ArgumentResult(false, argumentValues[0], message)
				);
			}

			// Check the signer key
			NyzoString.NyzoString signerSeed = NyzoStringEncoder.Decode(argumentValues[1]);

			if(signerSeed.GetType().IsInstanceOfType(typeof(NyzoStringPrivateSeed))) {
				argumentResults.Add(
					new ArgumentResult(true, NyzoStringEncoder.Encode(signerSeed))
				);
			} else {
				string message = String.IsNullOrEmpty(argumentValues[1]?.Trim()?.ToString() ?? "") ? "missing Nyzo string private key" : "not a valid Nyzo string private key";

				argumentResults.Add(
					new ArgumentResult(false, argumentValues[1], message)
				);

				if (argumentValues[1]?.Length ?? 0 >= 64) {
					PrivateNyzoStringCommand.PrintHexWarning(commandOutput);
				}
			}

			// Check the vote, it must either be 0 or 1
			string voteString = argumentValues[2];

			if(voteString == "0" || voteString == "1") {
				argumentResults.Add(
					new ArgumentResult(true, voteString)
				);
			} else {
				argumentResults.Add(
					new ArgumentResult(false, voteString, "Only 0 and 1 are valid vote values")
				);
			}

			result = new ValidationResult(argumentResults);

		} catch (Exception e) {
			LogUtil.PrintLine($"Failed to process {nameof(CycleTransactionSignCommand)}, args: {argumentValues}");
		}

		// If confirm result is null, create an exception result; this only happens if the exception is not handled properly by the validation code above
		if(result is null) {
			result = ValidationResult.FromExceptionResult(MethodInfo.GetCurrentMethod()?.ToString() + $", args: {argumentValues}");
		}

		return result;
	}
}
