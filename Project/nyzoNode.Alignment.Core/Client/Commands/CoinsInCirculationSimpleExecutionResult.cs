using nyzoNode.Alignment.Core.Web;
using System.Text;

namespace nyzoNode.Alignment.Core.Client.Commands;

public class CoinsInCirculationSimpleExecutionResult : SimpleExecutionResult {
	private double _totalCirculationNyzos { get; init; }

	// todo add base passthrough if needed in simplexcres
	public CoinsInCirculationSimpleExecutionResult(List<string> notices, List<string> errors, CommandTable commandTable, double totalCirculation): base(notices, errors, commandTable) {
		_totalCirculationNyzos = totalCirculation / (double) Transaction.MicroNyzoMultiplierRatio;
	}

	public override EndpointResponse EndpointResponse { get {
			var resultString = String.Format("{0:0.000000}", _totalCirculationNyzos);
			var resultBytes = Encoding.UTF8.GetBytes(resultString);

			return new EndpointResponse(resultBytes, EndpointResponse.ContentTypeText); // todo enum 2nd arg ?
		} 
	}
}