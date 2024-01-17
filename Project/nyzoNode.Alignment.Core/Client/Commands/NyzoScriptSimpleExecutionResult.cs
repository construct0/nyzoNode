using nyzoNode.Alignment.Core.Json;
using nyzoNode.Alignment.Core.NyzoScript;
using nyzoNode.Alignment.Core.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nyzoNode.Alignment.Core.Client.Commands;

internal class NyzoScriptSimpleExecutionResult : SimpleExecutionResult {
	private NyzoScriptState _nyzoScriptState { get; init; }
	private NyzoScriptCommand _instance { get; init; }

	// todo add base passthrough if needed in simplexcres
	public CoinsInCirculationSimpleExecutionResult(List<string> notices, List<string> errors, CommandTable commandTable, NyzoScriptState nyzoScriptState, NyzoScriptCommand instance) : base(notices, errors, commandTable) {
		_nyzoScriptState = nyzoScriptState;
		_instance = instance;
	}

	public override EndpointResponse EndpointResponse {
		get {
			EndpointResponse response;

			if(_nyzoScriptState is null) {
				return new EndpointResponse(
					JsonRenderer.ToJson(instance).GetBytes(Encoding.UTF8),
					EndpointResponse.ContentTypeJson
				);
			}

			return new EndpointResponse(
				_nyzoScriptState.RenderJson().GetBytes(Encoding.UTF8), // todo or prop
				EndpointResponse.ContentTypeJson
			);
		}
	}
}
