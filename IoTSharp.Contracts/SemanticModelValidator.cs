namespace IoTSharp.Contracts.Semantic;

/// <summary>
/// Validation rules for the open Semantic Core v1 contract.
/// </summary>
public static class SemanticModelValidator
{
    /// <summary>
    /// Validates the model and returns all diagnostics. The validator only checks model shape and semantic references;
    /// it never evaluates live telemetry, attributes, events, or alarm payloads.
    /// </summary>
    public static IReadOnlyList<SemanticValidationDiagnostic> Validate(SemanticModel? model)
    {
        var diagnostics = new List<SemanticValidationDiagnostic>();

        if (model is null)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                Severity: SemanticValidationSeverity.Error,
                Code: SemanticValidationCodes.ModelRequired,
                Path: "$",
                Message: "SemanticModel is required."));
            return diagnostics;
        }

        ValidateRequired(diagnostics, model.SchemaVersion, "$.schemaVersion", SemanticValidationCodes.ModelSchemaVersionRequired, "schemaVersion is required.");
        ValidateRequired(diagnostics, model.ModelId, "$.modelId", SemanticValidationCodes.ModelIdRequired, "modelId is required.");
        ValidateRequired(diagnostics, model.Name, "$.name", SemanticValidationCodes.ModelNameRequired, "name is required.");

        var assetIds = model.Assets
            .Select(asset => asset.AssetId)
            .Where(assetId => !string.IsNullOrWhiteSpace(assetId))
            .ToHashSet(StringComparer.Ordinal);

        var assetsById = model.Assets
            .Where(asset => !string.IsNullOrWhiteSpace(asset.AssetId))
            .GroupBy(asset => asset.AssetId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);

        var bindingIds = model.ProtocolBindings
            .Select(binding => binding.BindingId)
            .Where(bindingId => !string.IsNullOrWhiteSpace(bindingId))
            .ToHashSet(StringComparer.Ordinal);

        var bindingsById = model.ProtocolBindings
            .Where(binding => !string.IsNullOrWhiteSpace(binding.BindingId))
            .GroupBy(binding => binding.BindingId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);

        var referencedBindingIds = model.SemanticPoints
            .Select(point => point.Source.BindingId)
            .Where(bindingId => !string.IsNullOrWhiteSpace(bindingId))
            .ToHashSet(StringComparer.Ordinal);

        var seenAssetIds = new HashSet<string>(StringComparer.Ordinal);
        var seenAssetPaths = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < model.Assets.Count; index++)
        {
            ValidateAsset(model.Assets[index], index, assetIds, seenAssetIds, seenAssetPaths, diagnostics);
        }

        ValidateAssetHierarchy(model.Assets, diagnostics);

        var seenBindingIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < model.ProtocolBindings.Count; index++)
        {
            ValidateProtocolBinding(model.ProtocolBindings[index], index, referencedBindingIds, seenBindingIds, diagnostics);
        }

        var seenSemanticIds = new HashSet<string>(StringComparer.Ordinal);
        var pointOwners = new Dictionary<string, string>(StringComparer.Ordinal);
        var semanticPointsById = model.SemanticPoints
            .Where(point => !string.IsNullOrWhiteSpace(point.SemanticId))
            .GroupBy(point => point.SemanticId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        for (var index = 0; index < model.SemanticPoints.Count; index++)
        {
            ValidateSemanticPoint(model.SemanticPoints[index], index, assetIds, assetsById, bindingsById, seenSemanticIds, pointOwners, diagnostics);
        }

        ValidateAssetPointOwnership(model.Assets, pointOwners, diagnostics);
        ValidateProcessGraph(model.ProcessGraph, assetIds, seenSemanticIds, semanticPointsById, diagnostics);

        return diagnostics;
    }

    /// <summary>
    /// Returns true when <see cref="Validate"/> finds no error diagnostics.
    /// </summary>
    public static bool IsValid(SemanticModel? model)
        => Validate(model).All(diagnostic => diagnostic.Severity != SemanticValidationSeverity.Error);

    private static void ValidateAsset(
        Asset asset,
        int index,
        ISet<string> assetIds,
        ISet<string> seenAssetIds,
        ISet<string> seenAssetPaths,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"$.assets[{index}]";

        if (string.IsNullOrWhiteSpace(asset.AssetId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AssetIdRequired,
                $"{path}.assetId",
                "assetId is required for every L2 asset."));
        }
        else if (!seenAssetIds.Add(asset.AssetId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AssetIdDuplicate,
                $"{path}.assetId",
                $"assetId '{asset.AssetId}' is duplicated."));
        }

        ValidateRequired(diagnostics, asset.Name, $"{path}.name", SemanticValidationCodes.AssetNameRequired, "name is required for every L2 asset.");

        if (asset.AssetPath.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AssetPathRequired,
                $"{path}.assetPath",
                "assetPath must contain at least one stable path segment."));
        }
        else
        {
            for (var segmentIndex = 0; segmentIndex < asset.AssetPath.Count; segmentIndex++)
            {
                var segment = asset.AssetPath[segmentIndex];
                if (!IsValidAssetPathSegment(segment))
                {
                    diagnostics.Add(new SemanticValidationDiagnostic(
                        SemanticValidationSeverity.Error,
                        SemanticValidationCodes.AssetPathSegmentInvalid,
                        $"{path}.assetPath[{segmentIndex}]",
                        "assetPath segments must be lowercase code-friendly identifiers using letters, digits, '.', '_' or '-'."));
                }
            }

            var normalizedPath = string.Join("/", asset.AssetPath);
            if (!seenAssetPaths.Add(normalizedPath))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AssetPathDuplicate,
                    $"{path}.assetPath",
                    $"assetPath '{normalizedPath}' is duplicated."));
            }
        }

        if (!string.IsNullOrWhiteSpace(asset.ParentAssetId))
        {
            if (string.Equals(asset.ParentAssetId, asset.AssetId, StringComparison.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AssetParentSelf,
                    $"{path}.parentAssetId",
                    "parentAssetId must not point to the same asset."));
            }
            else if (!assetIds.Contains(asset.ParentAssetId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AssetParentMissing,
                    $"{path}.parentAssetId",
                    $"parentAssetId '{asset.ParentAssetId}' does not exist in assets."));
            }
        }

        for (var referenceIndex = 0; referenceIndex < asset.ExternalReferences.Count; referenceIndex++)
        {
            ValidateAssetExternalReference(asset.ExternalReferences[referenceIndex], $"{path}.externalReferences[{referenceIndex}]", diagnostics);
        }
    }

    private static void ValidateAssetExternalReference(
        AssetExternalReference reference,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        ValidateRequired(diagnostics, reference.ReferenceType, $"{path}.referenceType", SemanticValidationCodes.AssetExternalReferenceTypeRequired, "externalReferences[].referenceType is required.");
        ValidateRequired(diagnostics, reference.ReferenceId, $"{path}.referenceId", SemanticValidationCodes.AssetExternalReferenceIdRequired, "externalReferences[].referenceId is required.");

        if (!string.IsNullOrWhiteSpace(reference.Uri)
            && (!Uri.TryCreate(reference.Uri, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AssetExternalReferenceUriInvalid,
                $"{path}.uri",
                "externalReferences[].uri must be an absolute http or https jump URL when present."));
        }

        ValidateExternalReferenceMetadata(reference.Metadata, $"{path}.metadata", diagnostics);
    }

    private static void ValidateAssetHierarchy(
        IReadOnlyList<Asset> assets,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var assetsById = assets
            .Where(asset => !string.IsNullOrWhiteSpace(asset.AssetId))
            .GroupBy(asset => asset.AssetId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);

        for (var index = 0; index < assets.Count; index++)
        {
            var asset = assets[index];
            if (string.IsNullOrWhiteSpace(asset.AssetId) || string.IsNullOrWhiteSpace(asset.ParentAssetId))
            {
                continue;
            }

            if (!assetsById.TryGetValue(asset.ParentAssetId, out var parent))
            {
                continue;
            }

            if (!IsParentPathPrefix(parent.AssetPath, asset.AssetPath))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AssetParentPathMismatch,
                    $"$.assets[{index}].assetPath",
                    $"assetPath must extend parent asset '{asset.ParentAssetId}' path."));
            }

            var visited = new HashSet<string>(StringComparer.Ordinal) { asset.AssetId };
            var current = parent;
            while (!string.IsNullOrWhiteSpace(current.ParentAssetId))
            {
                if (!visited.Add(current.ParentAssetId))
                {
                    diagnostics.Add(new SemanticValidationDiagnostic(
                        SemanticValidationSeverity.Error,
                        SemanticValidationCodes.AssetParentCycle,
                        $"$.assets[{index}].parentAssetId",
                        "asset hierarchy contains a parentAssetId cycle."));
                    break;
                }

                if (!assetsById.TryGetValue(current.ParentAssetId, out current!))
                {
                    break;
                }
            }
        }
    }

    private static void ValidateProtocolBinding(
        ProtocolBinding binding,
        int index,
        ISet<string> referencedBindingIds,
        ISet<string> seenBindingIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"$.protocolBindings[{index}]";

        if (string.IsNullOrWhiteSpace(binding.BindingId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingIdRequired,
                $"{path}.bindingId",
                "bindingId is required for every L0 protocol binding."));
        }
        else
        {
            if (!seenBindingIds.Add(binding.BindingId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProtocolBindingIdDuplicate,
                    $"{path}.bindingId",
                    $"bindingId '{binding.BindingId}' is duplicated."));
            }

            if (!referencedBindingIds.Contains(binding.BindingId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProtocolBindingUnused,
                    $"{path}.bindingId",
                    $"bindingId '{binding.BindingId}' must be referenced by at least one semanticPoint.source.bindingId."));
            }
        }

        ValidateRequired(diagnostics, binding.Address, $"{path}.address", SemanticValidationCodes.ProtocolBindingAddressRequired, "address is required for every L0 protocol binding.");
        ValidateNonSecretString(binding.EndpointRef, $"{path}.endpointRef", diagnostics);
        ValidateNonSecretString(binding.Address, $"{path}.address", diagnostics);
        ValidateNonSecretString(binding.FieldPath, $"{path}.fieldPath", diagnostics);
        ValidateNonSecretMap(binding.Options, $"{path}.options", diagnostics);
        ValidateNonSecretMap(binding.Metadata, $"{path}.metadata", diagnostics);

        if (binding.Decode is not null)
        {
            ValidateNonSecretString(binding.Decode.ByteOrder, $"{path}.decode.byteOrder", diagnostics);
            ValidateNonSecretString(binding.Decode.WordOrder, $"{path}.decode.wordOrder", diagnostics);
            ValidateNonSecretString(binding.Decode.Encoding, $"{path}.decode.encoding", diagnostics);
            ValidateNonSecretMap(binding.Decode.Options, $"{path}.decode.options", diagnostics);
        }

        ValidateModbusBinding(binding, path, diagnostics);
        ValidateMqttBinding(binding, path, diagnostics);
        ValidateOpcUaBinding(binding, path, diagnostics);

        if (binding.Quality is not null)
        {
            ValidateNonSecretString(binding.Quality.Source, $"{path}.quality.source", diagnostics);
            ValidateNonSecretString(binding.Quality.FieldPath, $"{path}.quality.fieldPath", diagnostics);
            ValidateNonSecretString(binding.Quality.Reason, $"{path}.quality.reason", diagnostics);
            ValidateNonSecretMap(binding.Quality.Metadata, $"{path}.quality.metadata", diagnostics);
        }
    }

    private static void ValidateSemanticPoint(
        SemanticPoint point,
        int index,
        ISet<string> assetIds,
        IReadOnlyDictionary<string, Asset> assetsById,
        IReadOnlyDictionary<string, ProtocolBinding> bindingsById,
        ISet<string> seenSemanticIds,
        IDictionary<string, string> pointOwners,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"$.semanticPoints[{index}]";

        if (string.IsNullOrWhiteSpace(point.SemanticId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.SemanticPointIdRequired,
                $"{path}.semanticId",
                "semanticId is required for every L1 semantic point."));
        }
        else if (!seenSemanticIds.Add(point.SemanticId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.SemanticPointIdDuplicate,
                $"{path}.semanticId",
                $"semanticId '{point.SemanticId}' is duplicated."));
        }

        ValidateRequired(diagnostics, point.Name, $"{path}.name", SemanticValidationCodes.SemanticPointNameRequired, "name is required for every L1 semantic point.");
        ValidateRequired(diagnostics, point.Quantity.QuantityKind, $"{path}.quantity.quantityKind", SemanticValidationCodes.SemanticPointQuantityKindRequired, "quantity.quantityKind is required for every L1 semantic point.");
        ValidateRequired(diagnostics, point.Unit.Code, $"{path}.unit.code", SemanticValidationCodes.SemanticPointUnitRequired, "unit.code is required for every L1 semantic point.");

        if (string.IsNullOrWhiteSpace(point.AssetId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.SemanticPointAssetRequired,
                $"{path}.assetId",
                "assetId is required so every L1 semantic point has an L2 asset owner."));
        }
        else if (!assetIds.Contains(point.AssetId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.SemanticPointAssetMissing,
                $"{path}.assetId",
                $"assetId '{point.AssetId}' does not exist in assets."));
        }
        else if (!string.IsNullOrWhiteSpace(point.SemanticId))
        {
            pointOwners[point.SemanticId] = point.AssetId;
        }

        if (string.IsNullOrWhiteSpace(point.Source.BindingId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.SemanticPointSourceRequired,
                $"{path}.source.bindingId",
                "source.bindingId is required for every L1 semantic point."));
        }
        else if (bindingsById.Count > 0 && !bindingsById.ContainsKey(point.Source.BindingId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.SemanticPointSourceBindingMissing,
                $"{path}.source.bindingId",
                $"source binding '{point.Source.BindingId}' does not exist in protocolBindings."));
        }
        else if (bindingsById.TryGetValue(point.Source.BindingId, out var binding))
        {
            ValidateModbusPointAccess(point, binding, $"{path}.access", diagnostics);
            assetsById.TryGetValue(point.AssetId ?? string.Empty, out var asset);
            ValidateMqttPointBinding(point, binding, asset, $"{path}.source.bindingId", diagnostics);
            ValidateOpcUaPointBinding(point, binding, $"{path}.unit", diagnostics);
        }

        if (IsWritable(point.Access) && point.Unit.Code == "1" && string.Equals(point.Quantity.QuantityKind, "temperature", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Warning,
                SemanticValidationCodes.SemanticPointWritablePhysicalQuantity,
                $"{path}.access",
                "Writable physical points should be reviewed by a control policy in later L3 validation."));
        }
    }

    private static void ValidateAssetPointOwnership(
        IReadOnlyList<Asset> assets,
        IReadOnlyDictionary<string, string> pointOwners,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        for (var assetIndex = 0; assetIndex < assets.Count; assetIndex++)
        {
            var asset = assets[assetIndex];
            foreach (var semanticId in asset.Points.Where(point => !string.IsNullOrWhiteSpace(point)))
            {
                if (!pointOwners.TryGetValue(semanticId, out var ownerAssetId))
                {
                    diagnostics.Add(new SemanticValidationDiagnostic(
                        SemanticValidationSeverity.Error,
                        SemanticValidationCodes.AssetPointMissing,
                        $"$.assets[{assetIndex}].points",
                        $"asset point '{semanticId}' does not exist in semanticPoints."));
                    continue;
                }

                if (!string.Equals(ownerAssetId, asset.AssetId, StringComparison.Ordinal))
                {
                    diagnostics.Add(new SemanticValidationDiagnostic(
                        SemanticValidationSeverity.Error,
                        SemanticValidationCodes.AssetPointOwnershipMismatch,
                        $"$.assets[{assetIndex}].points",
                        $"asset point '{semanticId}' is owned by asset '{ownerAssetId}', not '{asset.AssetId}'."));
                }
            }
        }

        foreach (var (semanticId, ownerAssetId) in pointOwners)
        {
            var owner = assets.FirstOrDefault(asset => string.Equals(asset.AssetId, ownerAssetId, StringComparison.Ordinal));
            if (owner is not null && !owner.Points.Contains(semanticId, StringComparer.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.SemanticPointAssetMembershipMissing,
                    "$.semanticPoints",
                    $"semantic point '{semanticId}' declares assetId '{ownerAssetId}', but that asset does not list the point."));
            }
        }
    }

    private static bool IsWritable(SemanticPointAccess access)
        => access is SemanticPointAccess.Write or SemanticPointAccess.ReadWrite or SemanticPointAccess.Command or SemanticPointAccess.Config;

    private static void ValidateProcessGraph(
        ProcessGraph? processGraph,
        ISet<string> assetIds,
        ISet<string> semanticIds,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (processGraph is null)
        {
            return;
        }

        const string path = "$.processGraph";
        ValidateRequired(diagnostics, processGraph.ProcessGraphId, $"{path}.processGraphId", SemanticValidationCodes.ProcessGraphIdRequired, "processGraphId is required for every L3 process graph.");
        ValidateRequired(diagnostics, processGraph.Name, $"{path}.name", SemanticValidationCodes.ProcessGraphNameRequired, "name is required for every L3 process graph.");

        var seenNodeIds = new HashSet<string>(StringComparer.Ordinal);
        var validNodeIds = processGraph.Nodes
            .Select(node => node.NodeId)
            .Where(nodeId => !string.IsNullOrWhiteSpace(nodeId))
            .GroupBy(nodeId => nodeId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);

        for (var nodeIndex = 0; nodeIndex < processGraph.Nodes.Count; nodeIndex++)
        {
            ValidateProcessNode(
                processGraph.Nodes[nodeIndex],
                nodeIndex,
                assetIds,
                semanticIds,
                seenNodeIds,
                diagnostics);
        }

        var connectedNodeIds = new HashSet<string>(StringComparer.Ordinal);
        var mainFlowAdjacency = validNodeIds.ToDictionary(nodeId => nodeId, _ => new List<string>(), StringComparer.Ordinal);
        var seenEdgeIds = new HashSet<string>(StringComparer.Ordinal);
        for (var edgeIndex = 0; edgeIndex < processGraph.Edges.Count; edgeIndex++)
        {
            ValidateProcessEdge(
                processGraph.Edges[edgeIndex],
                edgeIndex,
                validNodeIds,
                seenEdgeIds,
                connectedNodeIds,
                mainFlowAdjacency,
                diagnostics);
        }

        ValidateProcessGraphConnectivity(processGraph.Nodes, connectedNodeIds, diagnostics);
        ValidateProcessGraphCycles(mainFlowAdjacency, diagnostics);
        ValidateDerivedPoints(processGraph.DerivedPoints, semanticIds, semanticPointsById, diagnostics);

        var validDerivedIds = processGraph.DerivedPoints
            .Select(point => point.SemanticId)
            .Where(semanticId => !string.IsNullOrWhiteSpace(semanticId))
            .GroupBy(semanticId => semanticId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);
        var stateModelsById = processGraph.StateModels
            .Where(model => !string.IsNullOrWhiteSpace(model.StateModelId))
            .GroupBy(model => model.StateModelId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);

        ValidateStateModels(processGraph.StateModels, assetIds, validNodeIds, semanticIds, validDerivedIds, diagnostics);
        ValidateAlarmSemantics(processGraph.Alarms, semanticIds, validDerivedIds, validNodeIds, stateModelsById, diagnostics);
        ValidateControlPolicies(processGraph.ControlPolicies, semanticPointsById, diagnostics);
    }

    private static void ValidateControlPolicies(
        IReadOnlyList<ControlPolicy> controlPolicies,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var policiesBySemanticId = new Dictionary<string, List<ControlPolicy>>(StringComparer.Ordinal);
        var seenPolicyIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < controlPolicies.Count; index++)
        {
            ValidateControlPolicy(
                controlPolicies[index],
                index,
                semanticPointsById,
                seenPolicyIds,
                policiesBySemanticId,
                diagnostics);
        }

        ValidateWritablePointsHaveControlPolicy(semanticPointsById, policiesBySemanticId, diagnostics);
    }

    private static void ValidateControlPolicy(
        ControlPolicy policy,
        int policyIndex,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        ISet<string> seenPolicyIds,
        IDictionary<string, List<ControlPolicy>> policiesBySemanticId,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"$.processGraph.controlPolicies[{policyIndex}]";
        if (string.IsNullOrWhiteSpace(policy.ControlPolicyId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyIdRequired,
                $"{path}.controlPolicyId",
                "controlPolicyId is required for every L3 control policy."));
        }
        else if (!seenPolicyIds.Add(policy.ControlPolicyId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyIdDuplicate,
                $"{path}.controlPolicyId",
                $"controlPolicyId '{policy.ControlPolicyId}' is duplicated."));
        }

        ValidateRequired(diagnostics, policy.Name, $"{path}.name", SemanticValidationCodes.ControlPolicyNameRequired, "name is required for every L3 control policy.");

        if (policy.AppliesToSemanticIds.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyTargetRequired,
                $"{path}.appliesToSemanticIds",
                "appliesToSemanticIds must contain at least one writable or command semantic point."));
        }

        if (!Enum.IsDefined(policy.Risk))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyRiskInvalid,
                $"{path}.risk",
                "risk must be normal or hazardous."));
        }

        if (!Enum.IsDefined(policy.AiOperationMode))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyAiOperationModeInvalid,
                $"{path}.aiOperationMode",
                "aiOperationMode must be recommendOnly, draftOnly, allowWithApproval, or allowAutomatic."));
        }

        var seenTargets = new HashSet<string>(StringComparer.Ordinal);
        for (var targetIndex = 0; targetIndex < policy.AppliesToSemanticIds.Count; targetIndex++)
        {
            var semanticId = policy.AppliesToSemanticIds[targetIndex];
            var targetPath = $"{path}.appliesToSemanticIds[{targetIndex}]";
            if (string.IsNullOrWhiteSpace(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ControlPolicyTargetRequired,
                    targetPath,
                    "control policy target semanticId must not be empty."));
                continue;
            }

            if (!seenTargets.Add(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ControlPolicyTargetDuplicate,
                    targetPath,
                    $"control policy target semanticId '{semanticId}' is duplicated."));
            }

            if (!semanticPointsById.TryGetValue(semanticId, out var point))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ControlPolicyTargetMissing,
                    targetPath,
                    $"control policy target semanticId '{semanticId}' does not exist in semanticPoints."));
                continue;
            }

            if (!IsWritable(point.Access))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ControlPolicyReadPointNotAllowed,
                    targetPath,
                    $"control policy '{policy.ControlPolicyId}' targets read-only semanticPoint '{semanticId}'."));
            }

            if (!policiesBySemanticId.TryGetValue(semanticId, out var policies))
            {
                policies = [];
                policiesBySemanticId[semanticId] = policies;
            }

            policies.Add(policy);
        }

        ValidateControlPolicyApprovalAndAiBoundary(policy, path, semanticPointsById, diagnostics);
    }

    private static void ValidateControlPolicyApprovalAndAiBoundary(
        ControlPolicy policy,
        string path,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var appliesToCommandOrConfig = policy.AppliesToSemanticIds
            .Select(semanticId => semanticPointsById.TryGetValue(semanticId, out var point) ? point : null)
            .OfType<SemanticPoint>()
            .Any(point => point.Access is SemanticPointAccess.Command or SemanticPointAccess.Config);
        var isHazardous = policy.Risk == ControlRisk.Hazardous;

        if ((isHazardous || appliesToCommandOrConfig) && !policy.RequiresApproval)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyApprovalRequired,
                $"{path}.requiresApproval",
                "hazardous, command, or config control policies must require human approval."));
        }

        if (policy.AiOperationMode == AiOperationMode.AllowWithApproval && !policy.RequiresApproval)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyApprovalRequired,
                $"{path}.requiresApproval",
                "aiOperationMode allowWithApproval requires requiresApproval to be true."));
        }

        if (policy.AllowAutomaticExecution && policy.AiOperationMode != AiOperationMode.AllowAutomatic)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyAutomaticExecutionNotAllowed,
                $"{path}.allowAutomaticExecution",
                "allowAutomaticExecution requires aiOperationMode to be allowAutomatic."));
        }

        if (policy.AiOperationMode == AiOperationMode.AllowAutomatic && !policy.AllowAutomaticExecution)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyAutomaticExecutionNotAllowed,
                $"{path}.aiOperationMode",
                "AI defaults to recommendations unless allowAutomaticExecution is explicitly true."));
        }

        if ((isHazardous || appliesToCommandOrConfig) && policy.AiOperationMode == AiOperationMode.AllowAutomatic)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ControlPolicyAutomaticExecutionNotAllowed,
                $"{path}.aiOperationMode",
                "hazardous, command, or config control policies must not allow automatic AI execution."));
        }
    }

    private static void ValidateWritablePointsHaveControlPolicy(
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        IReadOnlyDictionary<string, List<ControlPolicy>> policiesBySemanticId,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        foreach (var point in semanticPointsById.Values.OrderBy(point => point.SemanticId, StringComparer.Ordinal))
        {
            if (!IsWritable(point.Access))
            {
                continue;
            }

            var metadataPolicyId = TryGetMetadataString(point.Metadata, "controlPolicyId");
            var hasPolicyByTarget = policiesBySemanticId.TryGetValue(point.SemanticId, out var policies)
                && policies.Count > 0;
            var hasMetadataPolicy = !string.IsNullOrWhiteSpace(metadataPolicyId)
                && policiesBySemanticId.TryGetValue(point.SemanticId, out var metadataPolicies)
                && metadataPolicies.Any(policy => string.Equals(policy.ControlPolicyId, metadataPolicyId, StringComparison.Ordinal));

            if (!hasPolicyByTarget || (!string.IsNullOrWhiteSpace(metadataPolicyId) && !hasMetadataPolicy))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ControlPolicyWritablePointPolicyRequired,
                    "$.processGraph.controlPolicies",
                    $"writable semanticPoint '{point.SemanticId}' must be covered by a control policy."));
            }
        }
    }

    private static void ValidateStateModels(
        IReadOnlyList<StateModel> stateModels,
        ISet<string> assetIds,
        ISet<string> nodeIds,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var seenModelIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < stateModels.Count; index++)
        {
            ValidateStateModel(
                stateModels[index],
                index,
                assetIds,
                nodeIds,
                semanticIds,
                derivedIds,
                seenModelIds,
                diagnostics);
        }
    }

    private static void ValidateAlarmSemantics(
        IReadOnlyList<AlarmSemantics> alarms,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> nodeIds,
        IReadOnlyDictionary<string, StateModel> stateModelsById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var seenAlarmIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < alarms.Count; index++)
        {
            ValidateAlarmSemantics(
                alarms[index],
                index,
                semanticIds,
                derivedIds,
                nodeIds,
                stateModelsById,
                seenAlarmIds,
                diagnostics);
        }
    }

    private static void ValidateAlarmSemantics(
        AlarmSemantics alarm,
        int alarmIndex,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> nodeIds,
        IReadOnlyDictionary<string, StateModel> stateModelsById,
        ISet<string> seenAlarmIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"$.processGraph.alarms[{alarmIndex}]";
        if (string.IsNullOrWhiteSpace(alarm.AlarmId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AlarmIdRequired,
                $"{path}.alarmId",
                "alarmId is required for every L3 alarm semantics definition."));
        }
        else if (!seenAlarmIds.Add(alarm.AlarmId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AlarmIdDuplicate,
                $"{path}.alarmId",
                $"alarmId '{alarm.AlarmId}' is duplicated."));
        }

        ValidateRequired(diagnostics, alarm.Name, $"{path}.name", SemanticValidationCodes.AlarmNameRequired, "name is required for every L3 alarm semantics definition.");
        ValidateRequired(diagnostics, alarm.BusinessMeaning, $"{path}.businessMeaning", SemanticValidationCodes.AlarmBusinessMeaningRequired, "businessMeaning is required so the alarm has an explicit operational meaning.");
        ValidateRequired(diagnostics, alarm.Condition, $"{path}.condition", SemanticValidationCodes.AlarmConditionRequired, "condition is required for every L3 alarm semantics definition.");

        if (!Enum.IsDefined(alarm.Severity) || alarm.Severity == AlarmSeverity.Unknown)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AlarmSeverityRequired,
                $"{path}.severity",
                "severity must be one of info, warning, minor, major, or critical."));
        }

        if (alarm.Duration.HasValue && alarm.Duration.Value <= TimeSpan.Zero)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AlarmDurationInvalid,
                $"{path}.duration",
                "duration must be a positive time span when provided."));
        }

        ValidateAlarmDependencies(alarm, path, semanticIds, derivedIds, nodeIds, stateModelsById, diagnostics);
        ValidateAlarmConditions(alarm, path, semanticIds, derivedIds, nodeIds, stateModelsById, diagnostics);
    }

    private static void ValidateAlarmDependencies(
        AlarmSemantics alarm,
        string path,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> nodeIds,
        IReadOnlyDictionary<string, StateModel> stateModelsById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (alarm.DependsOnSemanticIds.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AlarmDependencyRequired,
                $"{path}.dependsOnSemanticIds",
                "dependsOnSemanticIds must contain the SemanticPoint or DerivedPoint IDs that give this alarm its business meaning."));
        }

        ValidateAlarmStringDependencies(
            alarm.DependsOnSemanticIds,
            $"{path}.dependsOnSemanticIds",
            SemanticValidationCodes.AlarmDependencyRequired,
            SemanticValidationCodes.AlarmDependencyDuplicate,
            dependencyId => semanticIds.Contains(dependencyId) || derivedIds.Contains(dependencyId),
            SemanticValidationCodes.AlarmSemanticReferenceMissing,
            "alarm semantic dependency",
            "semanticPoints or processGraph.derivedPoints",
            diagnostics);

        ValidateAlarmStringDependencies(
            alarm.DependsOnNodeIds,
            $"{path}.dependsOnNodeIds",
            SemanticValidationCodes.AlarmDependencyRequired,
            SemanticValidationCodes.AlarmDependencyDuplicate,
            nodeIds.Contains,
            SemanticValidationCodes.AlarmNodeReferenceMissing,
            "alarm node dependency",
            "processGraph.nodes",
            diagnostics);

        ValidateAlarmStringDependencies(
            alarm.DependsOnStateModelIds,
            $"{path}.dependsOnStateModelIds",
            SemanticValidationCodes.AlarmDependencyRequired,
            SemanticValidationCodes.AlarmDependencyDuplicate,
            stateModelsById.ContainsKey,
            SemanticValidationCodes.AlarmStateModelReferenceMissing,
            "alarm state model dependency",
            "processGraph.stateModels",
            diagnostics);
    }

    private static void ValidateAlarmStringDependencies(
        IReadOnlyList<string> dependencyIds,
        string path,
        string requiredCode,
        string duplicateCode,
        Func<string, bool> exists,
        string missingCode,
        string dependencyLabel,
        string targetLabel,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var seenDependencies = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < dependencyIds.Count; index++)
        {
            var dependencyId = dependencyIds[index];
            var dependencyPath = $"{path}[{index}]";
            if (string.IsNullOrWhiteSpace(dependencyId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    requiredCode,
                    dependencyPath,
                    $"{dependencyLabel} must not be empty."));
                continue;
            }

            if (!seenDependencies.Add(dependencyId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    duplicateCode,
                    dependencyPath,
                    $"{dependencyLabel} '{dependencyId}' is duplicated."));
            }

            if (!exists(dependencyId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    missingCode,
                    dependencyPath,
                    $"{dependencyLabel} '{dependencyId}' does not exist in {targetLabel}."));
            }
        }
    }

    private static void ValidateAlarmConditions(
        AlarmSemantics alarm,
        string path,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> nodeIds,
        IReadOnlyDictionary<string, StateModel> stateModelsById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var referencedSemanticIds = new HashSet<string>(StringComparer.Ordinal);
        var referencedNodeIds = new HashSet<string>(StringComparer.Ordinal);
        var referencedStateModelIds = new HashSet<string>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(alarm.Condition))
        {
            ValidateAlarmCondition(
                alarm,
                alarm.Condition,
                $"{path}.condition",
                semanticIds,
                derivedIds,
                nodeIds,
                stateModelsById,
                referencedSemanticIds,
                referencedNodeIds,
                referencedStateModelIds,
                diagnostics);
        }

        if (!string.IsNullOrWhiteSpace(alarm.SuppressionCondition))
        {
            ValidateAlarmCondition(
                alarm,
                alarm.SuppressionCondition,
                $"{path}.suppressionCondition",
                semanticIds,
                derivedIds,
                nodeIds,
                stateModelsById,
                referencedSemanticIds,
                referencedNodeIds,
                referencedStateModelIds,
                diagnostics);
        }

        if (!string.IsNullOrWhiteSpace(alarm.RecoveryCondition))
        {
            ValidateAlarmCondition(
                alarm,
                alarm.RecoveryCondition,
                $"{path}.recoveryCondition",
                semanticIds,
                derivedIds,
                nodeIds,
                stateModelsById,
                referencedSemanticIds,
                referencedNodeIds,
                referencedStateModelIds,
                diagnostics);
        }

        ValidateAlarmDependencyAlignment(
            alarm,
            path,
            referencedSemanticIds,
            referencedNodeIds,
            referencedStateModelIds,
            stateModelsById,
            diagnostics);
    }

    private static void ValidateAlarmCondition(
        AlarmSemantics alarm,
        string conditionText,
        string path,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> nodeIds,
        IReadOnlyDictionary<string, StateModel> stateModelsById,
        ISet<string> referencedSemanticIds,
        ISet<string> referencedNodeIds,
        ISet<string> referencedStateModelIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (!TryParseAlarmCondition(conditionText, out var condition, out var error))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AlarmConditionInvalid,
                path,
                $"alarm condition is invalid: {error}"));
            return;
        }

        if (condition.ReferencedSemanticIds.Count == 0
            && condition.ReferencedNodeIds.Count == 0
            && condition.ReferencedStateModelIds.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.AlarmConditionReferenceRequired,
                path,
                "alarm condition must reference SemanticPoint, DerivedPoint, ProcessNode, or StateModel with ref(\"semanticId\"), node(\"nodeId\"), or state(\"stateModelId\")."));
        }

        foreach (var semanticId in condition.ReferencedSemanticIds)
        {
            referencedSemanticIds.Add(semanticId);
            if (!semanticIds.Contains(semanticId) && !derivedIds.Contains(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AlarmSemanticReferenceMissing,
                    path,
                    $"alarm condition references semanticId '{semanticId}' that does not exist in semanticPoints or processGraph.derivedPoints."));
            }

            if (!alarm.DependsOnSemanticIds.Contains(semanticId, StringComparer.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AlarmDependencyMismatch,
                    path,
                    $"alarm condition references semanticId '{semanticId}' that is not declared in dependsOnSemanticIds."));
            }
        }

        foreach (var nodeId in condition.ReferencedNodeIds)
        {
            referencedNodeIds.Add(nodeId);
            if (!nodeIds.Contains(nodeId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AlarmNodeReferenceMissing,
                    path,
                    $"alarm condition references nodeId '{nodeId}' that does not exist in processGraph.nodes."));
            }

            if (!alarm.DependsOnNodeIds.Contains(nodeId, StringComparer.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AlarmDependencyMismatch,
                    path,
                    $"alarm condition references nodeId '{nodeId}' that is not declared in dependsOnNodeIds."));
            }
        }

        foreach (var stateModelId in condition.ReferencedStateModelIds)
        {
            referencedStateModelIds.Add(stateModelId);
            if (!stateModelsById.ContainsKey(stateModelId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AlarmStateModelReferenceMissing,
                    path,
                    $"alarm condition references stateModelId '{stateModelId}' that does not exist in processGraph.stateModels."));
            }

            if (!alarm.DependsOnStateModelIds.Contains(stateModelId, StringComparer.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AlarmDependencyMismatch,
                    path,
                    $"alarm condition references stateModelId '{stateModelId}' that is not declared in dependsOnStateModelIds."));
            }
        }
    }

    private static void ValidateAlarmDependencyAlignment(
        AlarmSemantics alarm,
        string path,
        ISet<string> referencedSemanticIds,
        ISet<string> referencedNodeIds,
        ISet<string> referencedStateModelIds,
        IReadOnlyDictionary<string, StateModel> stateModelsById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var stateModelSemanticDependencies = new HashSet<string>(StringComparer.Ordinal);
        var stateModelNodeDependencies = new HashSet<string>(StringComparer.Ordinal);
        foreach (var stateModelId in referencedStateModelIds)
        {
            if (stateModelsById.TryGetValue(stateModelId, out var stateModel))
            {
                AddStateModelDependencies(stateModel, stateModelSemanticDependencies, stateModelNodeDependencies);
            }
        }

        foreach (var semanticId in alarm.DependsOnSemanticIds)
        {
            if (!referencedSemanticIds.Contains(semanticId) && !stateModelSemanticDependencies.Contains(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AlarmDependencyMismatch,
                    $"{path}.dependsOnSemanticIds",
                    $"dependsOnSemanticIds includes semanticId '{semanticId}' that is not referenced by alarm conditions or referenced state models."));
            }
        }

        foreach (var nodeId in alarm.DependsOnNodeIds)
        {
            if (!referencedNodeIds.Contains(nodeId) && !stateModelNodeDependencies.Contains(nodeId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AlarmDependencyMismatch,
                    $"{path}.dependsOnNodeIds",
                    $"dependsOnNodeIds includes nodeId '{nodeId}' that is not referenced by alarm conditions or referenced state models."));
            }
        }

        foreach (var stateModelId in alarm.DependsOnStateModelIds)
        {
            if (!referencedStateModelIds.Contains(stateModelId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AlarmDependencyMismatch,
                    $"{path}.dependsOnStateModelIds",
                    $"dependsOnStateModelIds includes stateModelId '{stateModelId}' that is not referenced by alarm conditions."));
            }
        }
    }

    private static void AddStateModelDependencies(
        StateModel stateModel,
        ISet<string> semanticDependencies,
        ISet<string> nodeDependencies)
    {
        foreach (var state in stateModel.States)
        {
            semanticDependencies.UnionWith(state.DependsOnSemanticIds);
            nodeDependencies.UnionWith(state.DependsOnNodeIds);
        }

        foreach (var transition in stateModel.Transitions)
        {
            if (string.IsNullOrWhiteSpace(transition.Condition)
                || !TryParseStateCondition(transition.Condition, out var condition, out _))
            {
                continue;
            }

            semanticDependencies.UnionWith(condition.ReferencedSemanticIds);
            nodeDependencies.UnionWith(condition.ReferencedNodeIds);
        }
    }

    private static void ValidateStateModel(
        StateModel stateModel,
        int modelIndex,
        ISet<string> assetIds,
        ISet<string> nodeIds,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> seenModelIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"$.processGraph.stateModels[{modelIndex}]";
        if (string.IsNullOrWhiteSpace(stateModel.StateModelId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateModelIdRequired,
                $"{path}.stateModelId",
                "stateModelId is required for every L3 state model."));
        }
        else if (!seenModelIds.Add(stateModel.StateModelId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateModelIdDuplicate,
                $"{path}.stateModelId",
                $"stateModelId '{stateModel.StateModelId}' is duplicated."));
        }

        ValidateRequired(diagnostics, stateModel.Name, $"{path}.name", SemanticValidationCodes.StateModelNameRequired, "name is required for every L3 state model.");

        if (!string.IsNullOrWhiteSpace(stateModel.AppliesToAssetId) && !assetIds.Contains(stateModel.AppliesToAssetId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateModelAssetMissing,
                $"{path}.appliesToAssetId",
                $"state model appliesToAssetId '{stateModel.AppliesToAssetId}' does not exist in assets."));
        }

        if (!string.IsNullOrWhiteSpace(stateModel.AppliesToNodeId) && !nodeIds.Contains(stateModel.AppliesToNodeId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateModelNodeMissing,
                $"{path}.appliesToNodeId",
                $"state model appliesToNodeId '{stateModel.AppliesToNodeId}' does not exist in processGraph.nodes."));
        }

        if (stateModel.States.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateModelStateRequired,
                $"{path}.states",
                "states must contain at least one state definition."));
            return;
        }

        var stateIds = stateModel.States
            .Select(state => state.StateId)
            .Where(stateId => !string.IsNullOrWhiteSpace(stateId))
            .GroupBy(stateId => stateId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);

        ValidateRequired(diagnostics, stateModel.DefaultStateId, $"{path}.defaultStateId", SemanticValidationCodes.StateModelDefaultStateRequired, "defaultStateId is required so unmatched state conditions have a deterministic fallback.");
        if (!string.IsNullOrWhiteSpace(stateModel.DefaultStateId) && !stateIds.Contains(stateModel.DefaultStateId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateModelDefaultStateMissing,
                $"{path}.defaultStateId",
                $"defaultStateId '{stateModel.DefaultStateId}' does not exist in states[].stateId."));
        }

        var seenStateIds = new HashSet<string>(StringComparer.Ordinal);
        var seenStateKinds = new HashSet<ProcessStateKind>();
        var seenConditions = new HashSet<string>(StringComparer.Ordinal);
        for (var stateIndex = 0; stateIndex < stateModel.States.Count; stateIndex++)
        {
            ValidateStateDefinition(
                stateModel.States[stateIndex],
                stateIndex,
                path,
                stateModel.DefaultStateId,
                stateModel.MutuallyExclusive,
                semanticIds,
                derivedIds,
                nodeIds,
                seenStateIds,
                seenStateKinds,
                seenConditions,
                diagnostics);
        }

        for (var transitionIndex = 0; transitionIndex < stateModel.Transitions.Count; transitionIndex++)
        {
            ValidateStateTransition(
                stateModel.Transitions[transitionIndex],
                transitionIndex,
                path,
                stateIds,
                semanticIds,
                derivedIds,
                nodeIds,
                diagnostics);
        }
    }

    private static void ValidateStateDefinition(
        StateDefinition state,
        int stateIndex,
        string modelPath,
        string? defaultStateId,
        bool mutuallyExclusive,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> nodeIds,
        ISet<string> seenStateIds,
        ISet<ProcessStateKind> seenStateKinds,
        ISet<string> seenConditions,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"{modelPath}.states[{stateIndex}]";
        if (string.IsNullOrWhiteSpace(state.StateId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateDefinitionIdRequired,
                $"{path}.stateId",
                "stateId is required for every state definition."));
        }
        else if (!seenStateIds.Add(state.StateId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateDefinitionIdDuplicate,
                $"{path}.stateId",
                $"stateId '{state.StateId}' is duplicated."));
        }

        ValidateRequired(diagnostics, state.Name, $"{path}.name", SemanticValidationCodes.StateDefinitionNameRequired, "name is required for every state definition.");

        if (!Enum.IsDefined(state.StateKind) || state.StateKind == ProcessStateKind.Unknown)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateDefinitionKindInvalid,
                $"{path}.stateKind",
                "stateKind must be one of running, standby, fault, maintenance, manual, automatic, stopped, or custom."));
        }

        var isDefaultState = !string.IsNullOrWhiteSpace(defaultStateId)
            && string.Equals(state.StateId, defaultStateId, StringComparison.Ordinal);
        if (!isDefaultState && string.IsNullOrWhiteSpace(state.Condition))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateDefinitionConditionRequired,
                $"{path}.condition",
                $"non-default state '{state.StateId}' must declare a condition."));
        }

        if (!string.IsNullOrWhiteSpace(state.Condition))
        {
            ValidateStateCondition(state, path, semanticIds, derivedIds, nodeIds, diagnostics);
        }

        ValidateStateDependencies(state, path, semanticIds, derivedIds, nodeIds, diagnostics);

        if (!mutuallyExclusive)
        {
            return;
        }

        if (state.StateKind is not (ProcessStateKind.Unknown or ProcessStateKind.Custom)
            && !seenStateKinds.Add(state.StateKind))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateModelMutualExclusionViolation,
                $"{path}.stateKind",
                $"mutually exclusive state models must not define stateKind '{state.StateKind}' more than once."));
        }

        if (!string.IsNullOrWhiteSpace(state.Condition))
        {
            var normalizedCondition = NormalizeStateCondition(state.Condition);
            if (!seenConditions.Add(normalizedCondition))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateModelMutualExclusionViolation,
                    $"{path}.condition",
                    "mutually exclusive state models must not define duplicate state conditions."));
            }
        }
    }

    private static void ValidateStateCondition(
        StateDefinition state,
        string path,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> nodeIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (!TryParseStateCondition(state.Condition!, out var condition, out var error))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateDefinitionConditionInvalid,
                $"{path}.condition",
                $"state condition is invalid: {error}"));
            return;
        }

        if (condition.ReferencedSemanticIds.Count == 0 && condition.ReferencedNodeIds.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateDefinitionConditionReferenceRequired,
                $"{path}.condition",
                "state condition must reference SemanticPoint, DerivedPoint, or ProcessNode with ref(\"semanticId\") or node(\"nodeId\")."));
        }

        foreach (var semanticId in condition.ReferencedSemanticIds)
        {
            if (!semanticIds.Contains(semanticId) && !derivedIds.Contains(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionSemanticReferenceMissing,
                    $"{path}.condition",
                    $"state condition references semanticId '{semanticId}' that does not exist in semanticPoints or processGraph.derivedPoints."));
            }

            if (!state.DependsOnSemanticIds.Contains(semanticId, StringComparer.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionDependencyMismatch,
                    $"{path}.condition",
                    $"state condition references semanticId '{semanticId}' that is not declared in dependsOnSemanticIds."));
            }
        }

        foreach (var nodeId in condition.ReferencedNodeIds)
        {
            if (!nodeIds.Contains(nodeId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionNodeReferenceMissing,
                    $"{path}.condition",
                    $"state condition references nodeId '{nodeId}' that does not exist in processGraph.nodes."));
            }

            if (!state.DependsOnNodeIds.Contains(nodeId, StringComparer.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionDependencyMismatch,
                    $"{path}.condition",
                    $"state condition references nodeId '{nodeId}' that is not declared in dependsOnNodeIds."));
            }
        }

        foreach (var dependencyId in state.DependsOnSemanticIds)
        {
            if (!condition.ReferencedSemanticIds.Contains(dependencyId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionDependencyMismatch,
                    $"{path}.dependsOnSemanticIds",
                    $"dependsOnSemanticIds includes semanticId '{dependencyId}' that is not referenced by condition."));
            }
        }

        foreach (var nodeId in state.DependsOnNodeIds)
        {
            if (!condition.ReferencedNodeIds.Contains(nodeId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionDependencyMismatch,
                    $"{path}.dependsOnNodeIds",
                    $"dependsOnNodeIds includes nodeId '{nodeId}' that is not referenced by condition."));
            }
        }
    }

    private static void ValidateStateDependencies(
        StateDefinition state,
        string path,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> nodeIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var seenSemanticDependencies = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < state.DependsOnSemanticIds.Count; index++)
        {
            var semanticId = state.DependsOnSemanticIds[index];
            var dependencyPath = $"{path}.dependsOnSemanticIds[{index}]";
            if (string.IsNullOrWhiteSpace(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionDependencyRequired,
                    dependencyPath,
                    "state semantic dependency must not be empty."));
                continue;
            }

            if (!seenSemanticDependencies.Add(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionDependencyDuplicate,
                    dependencyPath,
                    $"state semantic dependency '{semanticId}' is duplicated."));
            }

            if (!semanticIds.Contains(semanticId) && !derivedIds.Contains(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionSemanticReferenceMissing,
                    dependencyPath,
                    $"state semantic dependency '{semanticId}' does not exist in semanticPoints or processGraph.derivedPoints."));
            }
        }

        var seenNodeDependencies = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < state.DependsOnNodeIds.Count; index++)
        {
            var nodeId = state.DependsOnNodeIds[index];
            var dependencyPath = $"{path}.dependsOnNodeIds[{index}]";
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionDependencyRequired,
                    dependencyPath,
                    "state node dependency must not be empty."));
                continue;
            }

            if (!seenNodeDependencies.Add(nodeId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionDependencyDuplicate,
                    dependencyPath,
                    $"state node dependency '{nodeId}' is duplicated."));
            }

            if (!nodeIds.Contains(nodeId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionNodeReferenceMissing,
                    dependencyPath,
                    $"state node dependency '{nodeId}' does not exist in processGraph.nodes."));
            }
        }
    }

    private static void ValidateStateTransition(
        StateTransition transition,
        int transitionIndex,
        string modelPath,
        ISet<string> stateIds,
        ISet<string> semanticIds,
        ISet<string> derivedIds,
        ISet<string> nodeIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"{modelPath}.transitions[{transitionIndex}]";
        ValidateRequired(diagnostics, transition.FromStateId, $"{path}.fromStateId", SemanticValidationCodes.StateTransitionStateRequired, "fromStateId is required for every state transition.");
        ValidateRequired(diagnostics, transition.ToStateId, $"{path}.toStateId", SemanticValidationCodes.StateTransitionStateRequired, "toStateId is required for every state transition.");

        if (!string.IsNullOrWhiteSpace(transition.FromStateId) && !stateIds.Contains(transition.FromStateId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateTransitionStateMissing,
                $"{path}.fromStateId",
                $"fromStateId '{transition.FromStateId}' does not exist in states[].stateId."));
        }

        if (!string.IsNullOrWhiteSpace(transition.ToStateId) && !stateIds.Contains(transition.ToStateId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateTransitionStateMissing,
                $"{path}.toStateId",
                $"toStateId '{transition.ToStateId}' does not exist in states[].stateId."));
        }

        if (string.IsNullOrWhiteSpace(transition.Condition))
        {
            return;
        }

        if (!TryParseStateCondition(transition.Condition, out var condition, out var error))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.StateDefinitionConditionInvalid,
                $"{path}.condition",
                $"state transition condition is invalid: {error}"));
            return;
        }

        foreach (var semanticId in condition.ReferencedSemanticIds)
        {
            if (!semanticIds.Contains(semanticId) && !derivedIds.Contains(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionSemanticReferenceMissing,
                    $"{path}.condition",
                    $"state transition condition references semanticId '{semanticId}' that does not exist in semanticPoints or processGraph.derivedPoints."));
            }
        }

        foreach (var nodeId in condition.ReferencedNodeIds)
        {
            if (!nodeIds.Contains(nodeId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.StateDefinitionNodeReferenceMissing,
                    $"{path}.condition",
                    $"state transition condition references nodeId '{nodeId}' that does not exist in processGraph.nodes."));
            }
        }
    }

    private static string NormalizeStateCondition(string condition)
        => string.Concat(condition.Where(character => !char.IsWhiteSpace(character))).ToLowerInvariant();

    private static bool TryParseStateCondition(
        string condition,
        out StateConditionParseResult result,
        out string error)
    {
        var parser = new StateConditionParser(condition);
        return parser.TryParse(out result, out error);
    }

    private static bool TryParseAlarmCondition(
        string condition,
        out StateConditionParseResult result,
        out string error)
    {
        var parser = new StateConditionParser(condition, allowStateReferences: true);
        return parser.TryParse(out result, out error);
    }

    private static void ValidateDerivedPoints(
        IReadOnlyList<DerivedPoint> derivedPoints,
        ISet<string> semanticIds,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var seenDerivedIds = new HashSet<string>(StringComparer.Ordinal);
        var derivedById = derivedPoints
            .Where(point => !string.IsNullOrWhiteSpace(point.SemanticId))
            .GroupBy(point => point.SemanticId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);

        for (var index = 0; index < derivedPoints.Count; index++)
        {
            ValidateDerivedPoint(
                derivedPoints[index],
                index,
                semanticIds,
                semanticPointsById,
                derivedById,
                seenDerivedIds,
                diagnostics);
        }

        ValidateDerivedPointCycles(derivedPoints, derivedById.Keys.ToHashSet(StringComparer.Ordinal), diagnostics);
    }

    private static void ValidateDerivedPoint(
        DerivedPoint point,
        int index,
        ISet<string> semanticIds,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        IReadOnlyDictionary<string, DerivedPoint> derivedById,
        ISet<string> seenDerivedIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"$.processGraph.derivedPoints[{index}]";

        if (string.IsNullOrWhiteSpace(point.SemanticId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointSemanticIdRequired,
                $"{path}.semanticId",
                "semanticId is required for every L3 derived point."));
        }
        else
        {
            if (semanticIds.Contains(point.SemanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointSemanticIdDuplicate,
                    $"{path}.semanticId",
                    $"derivedPoint semanticId '{point.SemanticId}' must not duplicate a semanticPoints[].semanticId."));
            }

            if (!seenDerivedIds.Add(point.SemanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointSemanticIdDuplicate,
                    $"{path}.semanticId",
                    $"derivedPoint semanticId '{point.SemanticId}' is duplicated."));
            }
        }

        ValidateRequired(diagnostics, point.Name, $"{path}.name", SemanticValidationCodes.DerivedPointNameRequired, "name is required for every L3 derived point.");
        ValidateRequired(diagnostics, point.Quantity.QuantityKind, $"{path}.quantity.quantityKind", SemanticValidationCodes.DerivedPointQuantityKindRequired, "quantity.quantityKind is required for every L3 derived point.");
        ValidateRequired(diagnostics, point.Unit.Code, $"{path}.unit.code", SemanticValidationCodes.DerivedPointUnitRequired, "unit.code is required for every L3 derived point.");

        var dependencyIds = ValidateDerivedPointDependencies(
            point,
            path,
            semanticIds,
            derivedById,
            diagnostics);

        ValidateDerivedPointExpression(
            point,
            path,
            dependencyIds,
            semanticPointsById,
            derivedById,
            diagnostics);

        ValidateDerivedPointRefresh(point, path, diagnostics);
    }

    private static HashSet<string> ValidateDerivedPointDependencies(
        DerivedPoint point,
        string path,
        ISet<string> semanticIds,
        IReadOnlyDictionary<string, DerivedPoint> derivedById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var dependencyIds = new HashSet<string>(StringComparer.Ordinal);
        if (point.DependsOnSemanticIds.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointDependencyRequired,
                $"{path}.dependsOnSemanticIds",
                "dependsOnSemanticIds must contain at least one semanticId used by the derived expression."));
            return dependencyIds;
        }

        for (var dependencyIndex = 0; dependencyIndex < point.DependsOnSemanticIds.Count; dependencyIndex++)
        {
            var dependencyId = point.DependsOnSemanticIds[dependencyIndex];
            var dependencyPath = $"{path}.dependsOnSemanticIds[{dependencyIndex}]";
            if (string.IsNullOrWhiteSpace(dependencyId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointDependencyRequired,
                    dependencyPath,
                    "derived point dependency semanticId must not be empty."));
                continue;
            }

            if (!dependencyIds.Add(dependencyId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointDependencyDuplicate,
                    dependencyPath,
                    $"derived point dependency semanticId '{dependencyId}' is duplicated."));
            }

            if (string.Equals(dependencyId, point.SemanticId, StringComparison.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointCycle,
                    dependencyPath,
                    $"derivedPoint '{point.SemanticId}' must not depend on itself."));
                continue;
            }

            if (!semanticIds.Contains(dependencyId) && !derivedById.ContainsKey(dependencyId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointDependencyMissing,
                    dependencyPath,
                    $"derived point dependency semanticId '{dependencyId}' does not exist in semanticPoints or processGraph.derivedPoints."));
            }
        }

        return dependencyIds;
    }

    private static void ValidateDerivedPointExpression(
        DerivedPoint point,
        string path,
        ISet<string> dependencyIds,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        IReadOnlyDictionary<string, DerivedPoint> derivedById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(point.Expression))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointExpressionRequired,
                $"{path}.expression",
                "expression is required for every L3 derived point."));
            return;
        }

        if (!TryParseDerivedExpression(point.Expression, out var expression, out var error))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointExpressionInvalid,
                $"{path}.expression",
                $"derived point expression is invalid: {error}"));
            return;
        }

        if (expression.ReferencedSemanticIds.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointExpressionSemanticReferenceRequired,
                $"{path}.expression",
                "expression must reference semantic IDs with ref(\"semanticId\")."));
            return;
        }

        foreach (var referencedSemanticId in expression.ReferencedSemanticIds)
        {
            if (!dependencyIds.Contains(referencedSemanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointExpressionDependencyMismatch,
                    $"{path}.expression",
                    $"expression references semanticId '{referencedSemanticId}' that is not declared in dependsOnSemanticIds."));
            }
        }

        foreach (var dependencyId in dependencyIds)
        {
            if (!expression.ReferencedSemanticIds.Contains(dependencyId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointExpressionDependencyMismatch,
                    $"{path}.dependsOnSemanticIds",
                    $"dependsOnSemanticIds includes semanticId '{dependencyId}' that is not referenced by expression."));
            }
        }

        ValidateDerivedPointUnitCompatibility(point, path, dependencyIds, semanticPointsById, derivedById, expression, diagnostics);
    }

    private static void ValidateDerivedPointUnitCompatibility(
        DerivedPoint point,
        string path,
        ISet<string> dependencyIds,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        IReadOnlyDictionary<string, DerivedPoint> derivedById,
        DerivedExpressionParseResult expression,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (expression.HasDivision && IsDimensionless(point.Unit))
        {
            ValidateDimensionlessDerivedPointDependencies(point, path, dependencyIds, semanticPointsById, derivedById, diagnostics);
            return;
        }

        foreach (var dependencyId in dependencyIds)
        {
            if (!TryGetReferencedUnit(dependencyId, semanticPointsById, derivedById, out var dependencyUnit))
            {
                continue;
            }

            if (!AreUnitsCompatible(point.Unit, dependencyUnit))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointUnitIncompatible,
                    $"{path}.unit",
                    $"derivedPoint unit '{point.Unit.Code}' is not compatible with dependency '{dependencyId}' unit '{dependencyUnit.Code}'."));
            }
        }
    }

    private static void ValidateDimensionlessDerivedPointDependencies(
        DerivedPoint point,
        string path,
        ISet<string> dependencyIds,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        IReadOnlyDictionary<string, DerivedPoint> derivedById,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        Unit? referenceUnit = null;
        string? referenceId = null;

        foreach (var dependencyId in dependencyIds)
        {
            if (!TryGetReferencedUnit(dependencyId, semanticPointsById, derivedById, out var dependencyUnit))
            {
                continue;
            }

            if (referenceUnit is null)
            {
                referenceUnit = dependencyUnit;
                referenceId = dependencyId;
                continue;
            }

            if (!AreUnitsCompatible(referenceUnit, dependencyUnit))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointUnitIncompatible,
                    $"{path}.unit",
                    $"dimensionless derivedPoint '{point.SemanticId}' divides incompatible dependency units '{referenceId}' ({referenceUnit.Code}) and '{dependencyId}' ({dependencyUnit.Code})."));
            }
        }

        if (referenceUnit is not null && dependencyIds.Count == 1 && !IsDimensionless(referenceUnit))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointUnitIncompatible,
                $"{path}.unit",
                $"dimensionless derivedPoint '{point.SemanticId}' must divide compatible units or depend only on dimensionless values."));
        }
    }

    private static void ValidateDerivedPointRefresh(
        DerivedPoint point,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(point.RefreshPolicy))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointRefreshPolicyRequired,
                $"{path}.refreshPolicy",
                "refreshPolicy is required for every L3 derived point."));
        }
        else if (!IsSupportedDerivedPointRefreshPolicy(point.RefreshPolicy))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointRefreshPolicyInvalid,
                $"{path}.refreshPolicy",
                "refreshPolicy must be onDependencyChange, fixedInterval, or manual."));
        }

        if (string.Equals(point.RefreshPolicy, "fixedInterval", StringComparison.Ordinal)
            && point.RefreshInterval is null)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointRefreshIntervalRequired,
                $"{path}.refreshInterval",
                "refreshInterval is required when refreshPolicy is fixedInterval."));
        }

        ValidatePositiveDuration(point.RefreshInterval, $"{path}.refreshInterval", diagnostics);
        ValidatePositiveDuration(point.StaleAfter, $"{path}.staleAfter", diagnostics);
        ValidatePositiveDuration(point.EvaluationWindow, $"{path}.evaluationWindow", diagnostics);
    }

    private static void ValidatePositiveDuration(
        TimeSpan? value,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (value.HasValue && value.Value <= TimeSpan.Zero)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.DerivedPointRefreshIntervalInvalid,
                path,
                "derived point refresh durations must be positive when present."));
        }
    }

    private static void ValidateDerivedPointCycles(
        IReadOnlyList<DerivedPoint> derivedPoints,
        ISet<string> derivedIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var graph = derivedPoints
            .Where(point => !string.IsNullOrWhiteSpace(point.SemanticId))
            .GroupBy(point => point.SemanticId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(
                group => group.Key,
                group => group.Single().DependsOnSemanticIds
                    .Where(derivedIds.Contains)
                    .Distinct(StringComparer.Ordinal)
                    .ToList(),
                StringComparer.Ordinal);

        var states = graph.Keys.ToDictionary(semanticId => semanticId, _ => ProcessNodeVisitState.NotVisited, StringComparer.Ordinal);
        foreach (var semanticId in graph.Keys)
        {
            if (states[semanticId] == ProcessNodeVisitState.NotVisited
                && VisitDerivedPoint(semanticId, graph, states, diagnostics))
            {
                return;
            }
        }
    }

    private static bool VisitDerivedPoint(
        string semanticId,
        IReadOnlyDictionary<string, List<string>> graph,
        IDictionary<string, ProcessNodeVisitState> states,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        states[semanticId] = ProcessNodeVisitState.Visiting;

        foreach (var next in graph[semanticId])
        {
            if (!graph.ContainsKey(next))
            {
                continue;
            }

            if (states[next] == ProcessNodeVisitState.NotVisited)
            {
                if (VisitDerivedPoint(next, graph, states, diagnostics))
                {
                    return true;
                }

                continue;
            }

            if (states[next] == ProcessNodeVisitState.Visiting)
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.DerivedPointCycle,
                    "$.processGraph.derivedPoints",
                    $"derived point calculation graph contains a cycle involving semanticId '{next}'."));
                return true;
            }
        }

        states[semanticId] = ProcessNodeVisitState.Visited;
        return false;
    }

    private static bool TryGetReferencedUnit(
        string semanticId,
        IReadOnlyDictionary<string, SemanticPoint> semanticPointsById,
        IReadOnlyDictionary<string, DerivedPoint> derivedById,
        out Unit unit)
    {
        if (semanticPointsById.TryGetValue(semanticId, out var semanticPoint))
        {
            unit = semanticPoint.Unit;
            return true;
        }

        if (derivedById.TryGetValue(semanticId, out var derivedPoint))
        {
            unit = derivedPoint.Unit;
            return true;
        }

        unit = new Unit();
        return false;
    }

    private static bool AreUnitsCompatible(Unit expected, Unit actual)
    {
        if (string.IsNullOrWhiteSpace(expected.Code) || string.IsNullOrWhiteSpace(actual.Code))
        {
            return false;
        }

        if (string.Equals(expected.Code, actual.Code, StringComparison.Ordinal)
            && expected.System == actual.System)
        {
            return true;
        }

        return IsDimensionless(expected) && IsDimensionless(actual);
    }

    private static bool IsDimensionless(Unit unit)
        => string.Equals(unit.Code, "1", StringComparison.Ordinal)
            || string.Equals(unit.QuantityKind, "dimensionless", StringComparison.OrdinalIgnoreCase)
            || string.Equals(unit.QuantityKind, "ratio", StringComparison.OrdinalIgnoreCase)
            || string.Equals(unit.QuantityKind, "efficiency", StringComparison.OrdinalIgnoreCase);

    private static bool IsSupportedDerivedPointRefreshPolicy(string refreshPolicy)
        => refreshPolicy is "onDependencyChange" or "fixedInterval" or "manual";

    private static bool TryParseDerivedExpression(
        string expression,
        out DerivedExpressionParseResult result,
        out string error)
    {
        var parser = new DerivedExpressionParser(expression);
        return parser.TryParse(out result, out error);
    }

    private static void ValidateProcessNode(
        ProcessNode node,
        int nodeIndex,
        ISet<string> assetIds,
        ISet<string> semanticIds,
        ISet<string> seenNodeIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"$.processGraph.nodes[{nodeIndex}]";

        if (string.IsNullOrWhiteSpace(node.NodeId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessNodeIdRequired,
                $"{path}.nodeId",
                "nodeId is required for every L3 process node."));
        }
        else if (!seenNodeIds.Add(node.NodeId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessNodeIdDuplicate,
                $"{path}.nodeId",
                $"process nodeId '{node.NodeId}' is duplicated."));
        }

        ValidateRequired(diagnostics, node.Name, $"{path}.name", SemanticValidationCodes.ProcessNodeNameRequired, "name is required for every L3 process node.");
        ValidateRequired(diagnostics, node.NodeType, $"{path}.nodeType", SemanticValidationCodes.ProcessNodeTypeRequired, "nodeType is required for every L3 process node.");

        if (string.IsNullOrWhiteSpace(node.AssetId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessNodeAssetRequired,
                $"{path}.assetId",
                "assetId is required so every L3 process node references an L2 asset."));
        }
        else if (!assetIds.Contains(node.AssetId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessNodeAssetMissing,
                $"{path}.assetId",
                $"process node assetId '{node.AssetId}' does not exist in assets."));
        }

        ValidateProcessNodeSemanticReferences(node.InputSemanticIds, $"{path}.inputSemanticIds", "input", semanticIds, diagnostics);
        ValidateProcessNodeSemanticReferences(node.OutputSemanticIds, $"{path}.outputSemanticIds", "output", semanticIds, diagnostics);

        if (node.InputSemanticIds.Count == 0 && node.OutputSemanticIds.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessNodePointReferenceRequired,
                path,
                $"process node '{node.NodeId}' must reference at least one inputSemanticIds or outputSemanticIds entry."));
        }
    }

    private static void ValidateProcessNodeSemanticReferences(
        IReadOnlyList<string> semanticReferences,
        string path,
        string role,
        ISet<string> semanticIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < semanticReferences.Count; index++)
        {
            var semanticId = semanticReferences[index];
            var itemPath = $"{path}[{index}]";
            if (string.IsNullOrWhiteSpace(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProcessNodeSemanticPointRequired,
                    itemPath,
                    $"process node {role} semantic point reference must not be empty."));
                continue;
            }

            if (!seen.Add(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProcessNodeSemanticPointDuplicate,
                    itemPath,
                    $"process node {role} semantic point '{semanticId}' is duplicated."));
            }

            if (!semanticIds.Contains(semanticId))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProcessNodeSemanticPointMissing,
                    itemPath,
                    $"process node {role} semantic point '{semanticId}' does not exist in semanticPoints."));
            }
        }
    }

    private static void ValidateProcessEdge(
        ProcessEdge edge,
        int edgeIndex,
        ISet<string> nodeIds,
        ISet<string> seenEdgeIds,
        ISet<string> connectedNodeIds,
        IDictionary<string, List<string>> mainFlowAdjacency,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var path = $"$.processGraph.edges[{edgeIndex}]";

        if (string.IsNullOrWhiteSpace(edge.EdgeId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessEdgeIdRequired,
                $"{path}.edgeId",
                "edgeId is required for every L3 process edge."));
        }
        else if (!seenEdgeIds.Add(edge.EdgeId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessEdgeIdDuplicate,
                $"{path}.edgeId",
                $"process edgeId '{edge.EdgeId}' is duplicated."));
        }

        var hasValidFrom = ValidateProcessEdgeNodeReference(edge.FromNodeId, $"{path}.fromNodeId", "fromNodeId", nodeIds, diagnostics);
        var hasValidTo = ValidateProcessEdgeNodeReference(edge.ToNodeId, $"{path}.toNodeId", "toNodeId", nodeIds, diagnostics);

        if (hasValidFrom && hasValidTo)
        {
            if (string.Equals(edge.FromNodeId, edge.ToNodeId, StringComparison.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProcessEdgeSelfReference,
                    path,
                    $"process edge '{edge.EdgeId}' must not point from a node to itself."));
            }
            else
            {
                connectedNodeIds.Add(edge.FromNodeId);
                connectedNodeIds.Add(edge.ToNodeId);

                if (IsMainProcessFlow(edge.Relation))
                {
                    mainFlowAdjacency[edge.FromNodeId].Add(edge.ToNodeId);
                }
            }
        }

        if (!Enum.IsDefined(edge.Relation))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessEdgeRelationInvalid,
                $"{path}.relation",
                $"process edge relation '{edge.Relation}' is not supported."));
        }
    }

    private static bool ValidateProcessEdgeNodeReference(
        string nodeId,
        string path,
        string role,
        ISet<string> nodeIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessEdgeNodeRequired,
                path,
                $"process edge {role} is required."));
            return false;
        }

        if (nodeIds.Contains(nodeId))
        {
            return true;
        }

        diagnostics.Add(new SemanticValidationDiagnostic(
            SemanticValidationSeverity.Error,
            SemanticValidationCodes.ProcessEdgeNodeMissing,
            path,
            $"process edge {role} '{nodeId}' does not exist in processGraph.nodes."));
        return false;
    }

    private static void ValidateProcessGraphConnectivity(
        IReadOnlyList<ProcessNode> nodes,
        ISet<string> connectedNodeIds,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (nodes.Count <= 1)
        {
            return;
        }

        for (var index = 0; index < nodes.Count; index++)
        {
            var node = nodes[index];
            if (string.IsNullOrWhiteSpace(node.NodeId) || connectedNodeIds.Contains(node.NodeId))
            {
                continue;
            }

            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProcessNodeDisconnected,
                $"$.processGraph.nodes[{index}]",
                $"process node '{node.NodeId}' is disconnected from the process graph."));
        }
    }

    private static void ValidateProcessGraphCycles(
        IReadOnlyDictionary<string, List<string>> mainFlowAdjacency,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        var states = mainFlowAdjacency.Keys.ToDictionary(nodeId => nodeId, _ => ProcessNodeVisitState.NotVisited, StringComparer.Ordinal);
        var stack = new Stack<string>();

        foreach (var nodeId in mainFlowAdjacency.Keys)
        {
            if (states[nodeId] == ProcessNodeVisitState.NotVisited
                && VisitProcessNode(nodeId, mainFlowAdjacency, states, stack, diagnostics))
            {
                return;
            }
        }
    }

    private static bool VisitProcessNode(
        string nodeId,
        IReadOnlyDictionary<string, List<string>> mainFlowAdjacency,
        IDictionary<string, ProcessNodeVisitState> states,
        Stack<string> stack,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        states[nodeId] = ProcessNodeVisitState.Visiting;
        stack.Push(nodeId);

        foreach (var next in mainFlowAdjacency[nodeId])
        {
            if (states[next] == ProcessNodeVisitState.NotVisited)
            {
                if (VisitProcessNode(next, mainFlowAdjacency, states, stack, diagnostics))
                {
                    return true;
                }

                continue;
            }

            if (states[next] == ProcessNodeVisitState.Visiting)
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProcessGraphCycle,
                    "$.processGraph.edges",
                    $"process graph contains a main-flow cycle involving node '{next}'."));
                return true;
            }
        }

        stack.Pop();
        states[nodeId] = ProcessNodeVisitState.Visited;
        return false;
    }

    private static bool IsMainProcessFlow(ProcessRelation relation)
        => relation is ProcessRelation.Input or ProcessRelation.Output or ProcessRelation.Upstream or ProcessRelation.Downstream or ProcessRelation.Dependency;

    private enum ProcessNodeVisitState
    {
        NotVisited,
        Visiting,
        Visited
    }

    private sealed record DerivedExpressionParseResult(
        HashSet<string> ReferencedSemanticIds,
        bool HasDivision);

    private sealed record StateConditionParseResult(
        HashSet<string> ReferencedSemanticIds,
        HashSet<string> ReferencedNodeIds,
        HashSet<string> ReferencedStateModelIds);

    private enum ConditionReferenceKind
    {
        Semantic,
        Node,
        StateModel
    }

    private sealed class StateConditionParser
    {
        private readonly string _condition;
        private readonly bool _allowStateReferences;
        private readonly HashSet<string> _referencedSemanticIds = new(StringComparer.Ordinal);
        private readonly HashSet<string> _referencedNodeIds = new(StringComparer.Ordinal);
        private readonly HashSet<string> _referencedStateModelIds = new(StringComparer.Ordinal);
        private int _position;

        public StateConditionParser(string condition, bool allowStateReferences = false)
        {
            _condition = condition;
            _allowStateReferences = allowStateReferences;
        }

        public bool TryParse(out StateConditionParseResult result, out string error)
        {
            result = new StateConditionParseResult([], [], []);
            error = string.Empty;

            SkipWhitespace();
            if (_position >= _condition.Length)
            {
                error = "condition must not be empty.";
                return false;
            }

            if (!ParseOr(out error))
            {
                return false;
            }

            SkipWhitespace();
            if (_position != _condition.Length)
            {
                error = $"unexpected token '{_condition[_position]}'.";
                return false;
            }

            result = new StateConditionParseResult(_referencedSemanticIds, _referencedNodeIds, _referencedStateModelIds);
            return true;
        }

        private bool ParseOr(out string error)
        {
            if (!ParseAnd(out error))
            {
                return false;
            }

            while (true)
            {
                SkipWhitespace();
                if (!Match("||"))
                {
                    return true;
                }

                if (!ParseAnd(out error))
                {
                    return false;
                }
            }
        }

        private bool ParseAnd(out string error)
        {
            if (!ParseNot(out error))
            {
                return false;
            }

            while (true)
            {
                SkipWhitespace();
                if (!Match("&&"))
                {
                    return true;
                }

                if (!ParseNot(out error))
                {
                    return false;
                }
            }
        }

        private bool ParseNot(out string error)
        {
            SkipWhitespace();
            if (Match("!"))
            {
                return ParseNot(out error);
            }

            return ParseComparison(out error);
        }

        private bool ParseComparison(out string error)
        {
            if (!ParseOperand(out error))
            {
                return false;
            }

            SkipWhitespace();
            if (Match("==") || Match("!=") || Match(">=") || Match("<=") || Match(">") || Match("<"))
            {
                return ParseOperand(out error);
            }

            return true;
        }

        private bool ParseOperand(out string error)
        {
            SkipWhitespace();
            if (_position >= _condition.Length)
            {
                error = "expected operand.";
                return false;
            }

            if (Match("("))
            {
                if (!ParseOr(out error))
                {
                    return false;
                }

                SkipWhitespace();
                if (!Match(")"))
                {
                    error = "missing closing parenthesis.";
                    return false;
                }

                return true;
            }

            if (char.IsDigit(Current) || Current == '-' || Current == '.')
            {
                return ParseNumber(out error);
            }

            if (Current == '"')
            {
                return ParseStringLiteral(out _, out error);
            }

            if (IsIdentifierStart(Current))
            {
                return ParseIdentifierOrReference(out error);
            }

            error = $"unexpected token '{Current}'.";
            return false;
        }

        private bool ParseIdentifierOrReference(out string error)
        {
            var start = _position;
            while (_position < _condition.Length && IsIdentifierPart(_condition[_position]))
            {
                _position++;
            }

            var identifier = _condition[start.._position];
            if (identifier is "true" or "false")
            {
                error = string.Empty;
                return true;
            }

            if (identifier is "ref" or "node" || (_allowStateReferences && identifier == "state"))
            {
                var referenceKind = identifier switch
                {
                    "ref" => ConditionReferenceKind.Semantic,
                    "node" => ConditionReferenceKind.Node,
                    _ => ConditionReferenceKind.StateModel
                };
                return ParseReference(referenceKind, out error);
            }

            error = $"unsupported identifier '{identifier}'. Use ref(\"semanticId\") or node(\"nodeId\") for state references.";
            return false;
        }

        private bool ParseReference(ConditionReferenceKind referenceKind, out string error)
        {
            SkipWhitespace();
            if (!Match("("))
            {
                error = referenceKind switch
                {
                    ConditionReferenceKind.Semantic => "ref must be called as ref(\"semanticId\").",
                    ConditionReferenceKind.Node => "node must be called as node(\"nodeId\").",
                    _ => "state must be called as state(\"stateModelId\")."
                };
                return false;
            }

            SkipWhitespace();
            if (!ParseStringLiteral(out var identifier, out error))
            {
                return false;
            }

            SkipWhitespace();
            if (!Match(")"))
            {
                error = "reference call is missing closing parenthesis.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(identifier))
            {
                error = "reference identifier must not be empty.";
                return false;
            }

            switch (referenceKind)
            {
                case ConditionReferenceKind.Semantic:
                    _referencedSemanticIds.Add(identifier);
                    break;
                case ConditionReferenceKind.Node:
                    _referencedNodeIds.Add(identifier);
                    break;
                default:
                    _referencedStateModelIds.Add(identifier);
                    break;
            }

            return true;
        }

        private bool ParseStringLiteral(out string value, out string error)
        {
            value = string.Empty;
            error = string.Empty;

            if (!Match("\""))
            {
                error = "string value must be a double-quoted string literal.";
                return false;
            }

            var builder = new System.Text.StringBuilder();
            while (_position < _condition.Length)
            {
                var character = _condition[_position++];
                if (character == '"')
                {
                    value = builder.ToString();
                    return true;
                }

                if (character == '\\')
                {
                    if (_position >= _condition.Length)
                    {
                        error = "unterminated escape sequence in string literal.";
                        return false;
                    }

                    var escaped = _condition[_position++];
                    if (escaped is '"' or '\\')
                    {
                        builder.Append(escaped);
                        continue;
                    }

                    error = "string literal only supports escaping quote and backslash characters.";
                    return false;
                }

                builder.Append(character);
            }

            error = "unterminated string literal.";
            return false;
        }

        private bool ParseNumber(out string error)
        {
            var start = _position;
            if (Current == '-')
            {
                _position++;
            }

            var seenDigit = false;
            while (_position < _condition.Length && char.IsDigit(_condition[_position]))
            {
                seenDigit = true;
                _position++;
            }

            if (_position < _condition.Length && _condition[_position] == '.')
            {
                _position++;
                while (_position < _condition.Length && char.IsDigit(_condition[_position]))
                {
                    seenDigit = true;
                    _position++;
                }
            }

            if (!seenDigit)
            {
                error = "number literal must contain at least one digit.";
                return false;
            }

            if (_position < _condition.Length && IsIdentifierStart(_condition[_position]))
            {
                error = $"unexpected identifier after number literal '{_condition[start.._position]}'.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private char Current => _position < _condition.Length ? _condition[_position] : '\0';

        private bool Match(string expected)
        {
            if (!_condition.AsSpan(_position).StartsWith(expected, StringComparison.Ordinal))
            {
                return false;
            }

            _position += expected.Length;
            return true;
        }

        private void SkipWhitespace()
        {
            while (_position < _condition.Length && char.IsWhiteSpace(_condition[_position]))
            {
                _position++;
            }
        }

        private static bool IsIdentifierStart(char value)
            => value is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';

        private static bool IsIdentifierPart(char value)
            => IsIdentifierStart(value) || value is >= '0' and <= '9';
    }

    private sealed class DerivedExpressionParser
    {
        private readonly string _expression;
        private readonly HashSet<string> _referencedSemanticIds = new(StringComparer.Ordinal);
        private int _position;
        private bool _hasDivision;

        public DerivedExpressionParser(string expression)
        {
            _expression = expression;
        }

        public bool TryParse(out DerivedExpressionParseResult result, out string error)
        {
            result = new DerivedExpressionParseResult([], false);
            error = string.Empty;

            SkipWhitespace();
            if (_position >= _expression.Length)
            {
                error = "expression must not be empty.";
                return false;
            }

            if (!ParseExpression(out error))
            {
                return false;
            }

            SkipWhitespace();
            if (_position != _expression.Length)
            {
                error = $"unexpected token '{_expression[_position]}'.";
                return false;
            }

            result = new DerivedExpressionParseResult(_referencedSemanticIds, _hasDivision);
            return true;
        }

        private bool ParseExpression(out string error)
        {
            if (!ParseTerm(out error))
            {
                return false;
            }

            while (true)
            {
                SkipWhitespace();
                if (!Match('+') && !Match('-'))
                {
                    return true;
                }

                if (!ParseTerm(out error))
                {
                    return false;
                }
            }
        }

        private bool ParseTerm(out string error)
        {
            if (!ParseUnary(out error))
            {
                return false;
            }

            while (true)
            {
                SkipWhitespace();
                if (Match('*'))
                {
                    if (!ParseUnary(out error))
                    {
                        return false;
                    }

                    continue;
                }

                if (Match('/'))
                {
                    _hasDivision = true;
                    if (!ParseUnary(out error))
                    {
                        return false;
                    }

                    continue;
                }

                return true;
            }
        }

        private bool ParseUnary(out string error)
        {
            SkipWhitespace();
            if (Match('+') || Match('-'))
            {
                return ParseUnary(out error);
            }

            return ParsePrimary(out error);
        }

        private bool ParsePrimary(out string error)
        {
            SkipWhitespace();
            if (_position >= _expression.Length)
            {
                error = "expected operand.";
                return false;
            }

            if (Match('('))
            {
                if (!ParseExpression(out error))
                {
                    return false;
                }

                SkipWhitespace();
                if (!Match(')'))
                {
                    error = "missing closing parenthesis.";
                    return false;
                }

                return true;
            }

            if (IsIdentifierStart(Current))
            {
                return ParseReference(out error);
            }

            if (char.IsDigit(Current) || Current == '.')
            {
                return ParseNumber(out error);
            }

            error = $"unexpected token '{Current}'.";
            return false;
        }

        private bool ParseReference(out string error)
        {
            var start = _position;
            while (_position < _expression.Length && IsIdentifierPart(_expression[_position]))
            {
                _position++;
            }

            var identifier = _expression[start.._position];
            if (!string.Equals(identifier, "ref", StringComparison.Ordinal))
            {
                error = $"unsupported identifier '{identifier}'. Use ref(\"semanticId\") for semantic references.";
                return false;
            }

            SkipWhitespace();
            if (!Match('('))
            {
                error = "ref must be called as ref(\"semanticId\").";
                return false;
            }

            SkipWhitespace();
            if (!ParseStringLiteral(out var semanticId, out error))
            {
                return false;
            }

            SkipWhitespace();
            if (!Match(')'))
            {
                error = "ref call is missing closing parenthesis.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(semanticId))
            {
                error = "ref semanticId must not be empty.";
                return false;
            }

            _referencedSemanticIds.Add(semanticId);
            return true;
        }

        private bool ParseStringLiteral(out string value, out string error)
        {
            value = string.Empty;
            error = string.Empty;

            if (!Match('"'))
            {
                error = "ref semanticId must be a double-quoted string literal.";
                return false;
            }

            var builder = new System.Text.StringBuilder();
            while (_position < _expression.Length)
            {
                var character = _expression[_position++];
                if (character == '"')
                {
                    value = builder.ToString();
                    return true;
                }

                if (character == '\\')
                {
                    if (_position >= _expression.Length)
                    {
                        error = "unterminated escape sequence in string literal.";
                        return false;
                    }

                    var escaped = _expression[_position++];
                    if (escaped is '"' or '\\')
                    {
                        builder.Append(escaped);
                        continue;
                    }

                    error = "string literal only supports escaping quote and backslash characters.";
                    return false;
                }

                builder.Append(character);
            }

            error = "unterminated string literal.";
            return false;
        }

        private bool ParseNumber(out string error)
        {
            var start = _position;
            var seenDigit = false;
            while (_position < _expression.Length && char.IsDigit(_expression[_position]))
            {
                seenDigit = true;
                _position++;
            }

            if (_position < _expression.Length && _expression[_position] == '.')
            {
                _position++;
                while (_position < _expression.Length && char.IsDigit(_expression[_position]))
                {
                    seenDigit = true;
                    _position++;
                }
            }

            if (!seenDigit)
            {
                error = "number literal must contain at least one digit.";
                return false;
            }

            if (_position < _expression.Length && IsIdentifierStart(_expression[_position]))
            {
                error = $"unexpected identifier after number literal '{_expression[start.._position]}'.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private char Current => _position < _expression.Length ? _expression[_position] : '\0';

        private bool Match(char expected)
        {
            if (_position >= _expression.Length || _expression[_position] != expected)
            {
                return false;
            }

            _position++;
            return true;
        }

        private void SkipWhitespace()
        {
            while (_position < _expression.Length && char.IsWhiteSpace(_expression[_position]))
            {
                _position++;
            }
        }

        private static bool IsIdentifierStart(char value)
            => value is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';

        private static bool IsIdentifierPart(char value)
            => IsIdentifierStart(value) || value is >= '0' and <= '9';
    }

    private static void ValidateModbusBinding(
        ProtocolBinding binding,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (binding.Modbus is null)
        {
            if (binding.ProtocolKind is SemanticProtocolKind.ModbusTcp or SemanticProtocolKind.ModbusRtu)
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProtocolBindingModbusMetadataRequired,
                    $"{path}.modbus",
                    "modbus binding metadata is required for modbusTcp and modbusRtu protocol bindings so functionCode, registerType, address, unitId, registerCount, byteOrder, wordOrder, scale, and offset are preserved."));
            }

            return;
        }

        var modbusPath = $"{path}.modbus";
        if (binding.ProtocolKind is not (SemanticProtocolKind.ModbusTcp or SemanticProtocolKind.ModbusRtu))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusKindMismatch,
                modbusPath,
                "modbus binding metadata is only valid for modbusTcp or modbusRtu protocol bindings."));
        }

        var modbus = binding.Modbus;
        if (!IsFunctionCodeAllowedForRegisterType(modbus.FunctionCode, modbus.RegisterType))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusFunctionCodeInvalid,
                $"{modbusPath}.functionCode",
                $"functionCode '{modbus.FunctionCode}' is not valid for registerType '{modbus.RegisterType}'."));
        }

        if (modbus.Address is < 0 or > 65535)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusAddressInvalid,
                $"{modbusPath}.address",
                "Modbus address must be a zero-based value from 0 to 65535."));
        }

        if (modbus.UnitId is < 0 or > 247)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusUnitIdInvalid,
                $"{modbusPath}.unitId",
                "Modbus unitId must be between 0 and 247."));
        }

        var maxRegisterCount = GetMaxRegisterCount(modbus.RegisterType);
        if (modbus.RegisterCount < 1 || modbus.RegisterCount > maxRegisterCount)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusRegisterCountInvalid,
                $"{modbusPath}.registerCount",
                $"registerCount must be between 1 and {maxRegisterCount} for registerType '{modbus.RegisterType}'."));
        }
        else if (IsSingleWriteFunctionCode(modbus.FunctionCode) && modbus.RegisterCount != 1)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusRegisterCountInvalid,
                $"{modbusPath}.registerCount",
                $"functionCode '{modbus.FunctionCode}' writes exactly one Modbus address."));
        }

        if (TryParseModbusAddress(binding.Address, out var addressRegisterType, out var address))
        {
            if (addressRegisterType.HasValue && addressRegisterType.Value != modbus.RegisterType)
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProtocolBindingModbusAddressMismatch,
                    $"{path}.address",
                    $"address register type '{addressRegisterType.Value}' does not match modbus.registerType '{modbus.RegisterType}'."));
            }

            if (address != modbus.Address)
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProtocolBindingModbusAddressMismatch,
                    $"{path}.address",
                    $"address '{binding.Address}' maps to zero-based Modbus address '{address}', not '{modbus.Address}'."));
            }
        }
        else
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusAddressInvalid,
                $"{path}.address",
                "Modbus address must be a numeric address or an area-prefixed address such as coil:00001, discrete-input:10001, input-register:30001, or holding-register:40001."));
        }
    }

    private static void ValidateModbusPointAccess(
        SemanticPoint point,
        ProtocolBinding binding,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (binding.Modbus is null)
        {
            return;
        }

        var modbus = binding.Modbus;
        if (IsWritable(point.Access) && !IsWritableRegisterType(modbus.RegisterType))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusAccessInvalid,
                path,
                $"registerType '{modbus.RegisterType}' is read-only and cannot back writable, command, or config semantic points."));
            return;
        }

        if (point.Access == SemanticPointAccess.Read && !IsReadFunctionCode(modbus.FunctionCode))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusAccessInvalid,
                path,
                $"read semantic points must use a Modbus read function code, not '{modbus.FunctionCode}'."));
        }

        if (point.Access is SemanticPointAccess.Write or SemanticPointAccess.Command or SemanticPointAccess.Config
            && !IsWriteFunctionCode(modbus.FunctionCode))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingModbusAccessInvalid,
                path,
                $"writable, command, and config semantic points must use a Modbus write function code, not '{modbus.FunctionCode}'."));
        }
    }

    private static void ValidateMqttBinding(
        ProtocolBinding binding,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (binding.Mqtt is null)
        {
            return;
        }

        var mqttPath = $"{path}.mqtt";
        if (binding.ProtocolKind != SemanticProtocolKind.Mqtt)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingMqttKindMismatch,
                mqttPath,
                "mqtt binding metadata is only valid for mqtt protocol bindings."));
        }

        var mqtt = binding.Mqtt;
        ValidateNonSecretString(mqtt.Topic, $"{mqttPath}.topic", diagnostics);
        ValidateNonSecretString(mqtt.TimestampField, $"{mqttPath}.timestampField", diagnostics);
        ValidateNonSecretString(mqtt.QualityField, $"{mqttPath}.qualityField", diagnostics);
        ValidateNonSecretString(mqtt.ValueField, $"{mqttPath}.valueField", diagnostics);

        if (string.IsNullOrWhiteSpace(mqtt.Topic) || !MqttUnsTopicBuilder.IsValidPublishTopic(mqtt.Topic))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingMqttTopicInvalid,
                $"{mqttPath}.topic",
                "MQTT topic must be a non-empty publish topic without wildcards, null characters, or empty hierarchy levels."));
        }
        else if (!string.IsNullOrWhiteSpace(binding.Address) && !string.Equals(binding.Address, mqtt.Topic, StringComparison.Ordinal))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingMqttTopicMismatch,
                $"{path}.address",
                "ProtocolBinding.address must match mqtt.topic for MQTT bindings."));
        }

        if (mqtt.NamespaceStyle == MqttNamespaceStyle.Uns && !MqttUnsTopicBuilder.IsValidUnsTopic(mqtt.Topic))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingMqttTopicInvalid,
                $"{mqttPath}.topic",
                "UNS MQTT topics must use the generated shape uns/{assetPath...}/{semanticId}."));
        }

        if (mqtt.Qos is < 0 or > 2)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingMqttQosInvalid,
                $"{mqttPath}.qos",
                "MQTT qos must be 0, 1, or 2."));
        }

        if (string.IsNullOrWhiteSpace(mqtt.ValueField))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingMqttValueFieldRequired,
                $"{mqttPath}.valueField",
                "valueField is required so the MQTT payload maps to a SemanticPoint value."));
        }

        if (mqtt.NamespaceStyle == MqttNamespaceStyle.Sparkplug && mqtt.SparkplugProfile is null)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingMqttSparkplugProfileRequired,
                $"{mqttPath}.sparkplugProfile",
                "sparkplugProfile is required when namespaceStyle is sparkplug."));
        }

        if (mqtt.SparkplugProfile is not null)
        {
            ValidateSparkplugProfile(mqtt.SparkplugProfile, $"{mqttPath}.sparkplugProfile", diagnostics);
        }
    }

    private static void ValidateMqttPointBinding(
        SemanticPoint point,
        ProtocolBinding binding,
        Asset? asset,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (binding.Mqtt is null || binding.Mqtt.NamespaceStyle != MqttNamespaceStyle.Uns || asset is null)
        {
            return;
        }

        try
        {
            var expectedTopic = MqttUnsTopicBuilder.GenerateTopic(asset.AssetPath, point.SemanticId);
            if (!string.Equals(binding.Mqtt.Topic, expectedTopic, StringComparison.Ordinal))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProtocolBindingMqttUnsTopicMismatch,
                    path,
                    $"UNS MQTT topic must be generated from assetPath and semanticId. Expected '{expectedTopic}'."));
            }
        }
        catch (ArgumentException exception)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingMqttTopicInvalid,
                path,
                exception.Message));
        }
    }

    private static void ValidateSparkplugProfile(
        SparkplugProfile profile,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        ValidateRequired(diagnostics, profile.GroupId, $"{path}.groupId", SemanticValidationCodes.ProtocolBindingMqttSparkplugProfileRequired, "sparkplugProfile.groupId is required.");
        ValidateRequired(diagnostics, profile.EdgeNodeId, $"{path}.edgeNodeId", SemanticValidationCodes.ProtocolBindingMqttSparkplugProfileRequired, "sparkplugProfile.edgeNodeId is required.");
        ValidateRequired(diagnostics, profile.DeviceId, $"{path}.deviceId", SemanticValidationCodes.ProtocolBindingMqttSparkplugProfileRequired, "sparkplugProfile.deviceId is required.");
        ValidateRequired(diagnostics, profile.MetricName, $"{path}.metricName", SemanticValidationCodes.ProtocolBindingMqttSparkplugProfileRequired, "sparkplugProfile.metricName is required.");

        ValidateNonSecretString(profile.GroupId, $"{path}.groupId", diagnostics);
        ValidateNonSecretString(profile.EdgeNodeId, $"{path}.edgeNodeId", diagnostics);
        ValidateNonSecretString(profile.DeviceId, $"{path}.deviceId", diagnostics);
        ValidateNonSecretString(profile.MetricName, $"{path}.metricName", diagnostics);

        ValidateSparkplugLifecycleState(profile.Birth, $"{path}.birth", diagnostics);
        ValidateSparkplugLifecycleState(profile.Death, $"{path}.death", diagnostics);
    }

    private static void ValidateSparkplugLifecycleState(
        SparkplugLifecycleState state,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        ValidateNonSecretString(state.Topic, $"{path}.topic", diagnostics);
        ValidateNonSecretString(state.MetricName, $"{path}.metricName", diagnostics);

        if (!string.IsNullOrWhiteSpace(state.Topic) && !MqttUnsTopicBuilder.IsValidPublishTopic(state.Topic))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingMqttTopicInvalid,
                $"{path}.topic",
                "Sparkplug lifecycle topics must be MQTT publish topics without wildcards, null characters, or empty hierarchy levels."));
        }
    }

    private static void ValidateOpcUaBinding(
        ProtocolBinding binding,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (binding.OpcUa is null)
        {
            if (binding.ProtocolKind == SemanticProtocolKind.OpcUa)
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProtocolBindingOpcUaMetadataRequired,
                    $"{path}.opcUa",
                    "opcUa binding metadata is required for opcUa protocol bindings so NodeId, BrowsePath, type, unit, and reference information are preserved."));
            }

            return;
        }

        var opcUaPath = $"{path}.opcUa";
        if (binding.ProtocolKind != SemanticProtocolKind.OpcUa)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingOpcUaKindMismatch,
                opcUaPath,
                "opcUa binding metadata is only valid for opcUa protocol bindings."));
        }

        var opcUa = binding.OpcUa;
        ValidateOpcUaNodeId(opcUa.NodeId, $"{opcUaPath}.nodeId", SemanticValidationCodes.ProtocolBindingOpcUaNodeIdRequired, diagnostics);
        ValidateOpcUaQualifiedName(opcUa.BrowseName, $"{opcUaPath}.browseName", SemanticValidationCodes.ProtocolBindingOpcUaBrowseNameRequired, diagnostics);
        ValidateOpcUaTypeReference(opcUa.DataType, $"{opcUaPath}.dataType", SemanticValidationCodes.ProtocolBindingOpcUaDataTypeRequired, diagnostics);
        ValidateNonSecretString(opcUa.DisplayName?.Text, $"{opcUaPath}.displayName.text", diagnostics);
        ValidateNonSecretString(opcUa.DisplayName?.Locale, $"{opcUaPath}.displayName.locale", diagnostics);

        if (!string.IsNullOrWhiteSpace(binding.Address)
            && !string.IsNullOrWhiteSpace(opcUa.NodeId.Text)
            && !string.Equals(binding.Address, opcUa.NodeId.Text, StringComparison.Ordinal))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingOpcUaNodeIdMismatch,
                $"{path}.address",
                "ProtocolBinding.address must match opcUa.nodeId.text for OPC UA bindings."));
        }

        if (opcUa.BrowsePath.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingOpcUaBrowsePathRequired,
                $"{opcUaPath}.browsePath",
                "browsePath must preserve the structured OPC UA browse path for imported nodes."));
        }
        else
        {
            for (var index = 0; index < opcUa.BrowsePath.Count; index++)
            {
                var browsePath = $"{opcUaPath}.browsePath[{index}]";
                var element = opcUa.BrowsePath[index];
                ValidateOpcUaQualifiedName(element.BrowseName, $"{browsePath}.browseName", SemanticValidationCodes.ProtocolBindingOpcUaBrowsePathRequired, diagnostics);

                if (element.ReferenceType is not null)
                {
                    ValidateOpcUaTypeReference(element.ReferenceType, $"{browsePath}.referenceType", SemanticValidationCodes.ProtocolBindingOpcUaReferenceTypeRequired, diagnostics);
                }
            }
        }

        if (opcUa.EngineeringUnits is not null)
        {
            ValidateNonSecretString(opcUa.EngineeringUnits.NamespaceUri, $"{opcUaPath}.engineeringUnits.namespaceUri", diagnostics);
            ValidateNonSecretString(opcUa.EngineeringUnits.DisplayName?.Text, $"{opcUaPath}.engineeringUnits.displayName.text", diagnostics);
            ValidateNonSecretString(opcUa.EngineeringUnits.DisplayName?.Locale, $"{opcUaPath}.engineeringUnits.displayName.locale", diagnostics);
            ValidateNonSecretString(opcUa.EngineeringUnits.Description?.Text, $"{opcUaPath}.engineeringUnits.description.text", diagnostics);
            ValidateNonSecretString(opcUa.EngineeringUnits.Description?.Locale, $"{opcUaPath}.engineeringUnits.description.locale", diagnostics);
        }

        if (opcUa.References.Count == 0)
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingOpcUaReferencesRequired,
                $"{opcUaPath}.references",
                "references must preserve the original OPC UA references used during Browse or NodeSet import."));
        }
        else
        {
            var hasTypeDefinition = false;
            for (var index = 0; index < opcUa.References.Count; index++)
            {
                var referencePath = $"{opcUaPath}.references[{index}]";
                var reference = opcUa.References[index];
                ValidateOpcUaTypeReference(reference.ReferenceType, $"{referencePath}.referenceType", SemanticValidationCodes.ProtocolBindingOpcUaReferenceTypeRequired, diagnostics);
                ValidateOpcUaNodeId(reference.TargetNodeId, $"{referencePath}.targetNodeId", SemanticValidationCodes.ProtocolBindingOpcUaReferenceTargetRequired, diagnostics);

                if (reference.TargetBrowseName is not null)
                {
                    ValidateOpcUaQualifiedName(reference.TargetBrowseName, $"{referencePath}.targetBrowseName", SemanticValidationCodes.ProtocolBindingOpcUaReferenceTargetRequired, diagnostics);
                }

                ValidateNonSecretString(reference.TargetDisplayName?.Text, $"{referencePath}.targetDisplayName.text", diagnostics);
                ValidateNonSecretString(reference.TargetDisplayName?.Locale, $"{referencePath}.targetDisplayName.locale", diagnostics);

                hasTypeDefinition |= IsOpcUaReferenceNamed(reference.ReferenceType, "HasTypeDefinition");
            }

            if (!hasTypeDefinition)
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProtocolBindingOpcUaTypeDefinitionReferenceRequired,
                    $"{opcUaPath}.references",
                    "references must include the original HasTypeDefinition reference for OPC UA nodes."));
            }
        }

        if (opcUa.ObjectType is not null)
        {
            ValidateOpcUaTypeReference(opcUa.ObjectType, $"{opcUaPath}.objectType", SemanticValidationCodes.ProtocolBindingOpcUaObjectTypeRequired, diagnostics);
        }

        if (opcUa.VariableType is not null)
        {
            ValidateOpcUaTypeReference(opcUa.VariableType, $"{opcUaPath}.variableType", SemanticValidationCodes.ProtocolBindingOpcUaVariableTypeRequired, diagnostics);
        }
    }

    private static void ValidateOpcUaPointBinding(
        SemanticPoint point,
        ProtocolBinding binding,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (binding.OpcUa?.EngineeringUnits is null)
        {
            return;
        }

        var unit = binding.OpcUa.EngineeringUnits;
        if (point.Unit.System == UnitSystem.OpcUa
            && unit.UnitId is null
            && string.IsNullOrWhiteSpace(unit.DisplayName?.Text)
            && string.IsNullOrWhiteSpace(unit.NamespaceUri))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingOpcUaEngineeringUnitsRequired,
                path,
                "OPC UA unit mappings must preserve engineeringUnits when SemanticPoint.unit.system is opcua."));
        }
    }

    private static void ValidateOpcUaNodeId(
        OpcUaNodeId nodeId,
        string path,
        string code,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        ValidateRequired(diagnostics, nodeId.Text, $"{path}.text", code, "OPC UA NodeId text is required.");
        ValidateRequired(diagnostics, nodeId.Identifier, $"{path}.identifier", code, "OPC UA NodeId identifier is required.");
        ValidateNonSecretString(nodeId.Text, $"{path}.text", diagnostics);
        ValidateNonSecretString(nodeId.Identifier, $"{path}.identifier", diagnostics);
        ValidateNonSecretString(nodeId.NamespaceUri, $"{path}.namespaceUri", diagnostics);

        if (!string.IsNullOrWhiteSpace(nodeId.Text) && !LooksLikeOpcUaNodeId(nodeId.Text))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingOpcUaNodeIdInvalid,
                $"{path}.text",
                "OPC UA NodeId text must use canonical syntax such as ns=2;s=Tag, ns=2;i=1234, ns=2;g={guid}, or ns=2;b={base64}."));
        }
    }

    private static void ValidateOpcUaQualifiedName(
        OpcUaQualifiedName qualifiedName,
        string path,
        string code,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        ValidateRequired(diagnostics, qualifiedName.Name, $"{path}.name", code, "OPC UA QualifiedName name is required.");
        ValidateNonSecretString(qualifiedName.Name, $"{path}.name", diagnostics);
        ValidateNonSecretString(qualifiedName.NamespaceUri, $"{path}.namespaceUri", diagnostics);
        ValidateNonSecretString(qualifiedName.Text, $"{path}.text", diagnostics);
    }

    private static void ValidateOpcUaTypeReference(
        OpcUaTypeReference reference,
        string path,
        string code,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        ValidateOpcUaNodeId(reference.NodeId, $"{path}.nodeId", code, diagnostics);

        if (reference.BrowseName is not null)
        {
            ValidateOpcUaQualifiedName(reference.BrowseName, $"{path}.browseName", code, diagnostics);
        }

        ValidateNonSecretString(reference.DisplayName?.Text, $"{path}.displayName.text", diagnostics);
        ValidateNonSecretString(reference.DisplayName?.Locale, $"{path}.displayName.locale", diagnostics);
    }

    private static bool IsOpcUaReferenceNamed(OpcUaTypeReference reference, string name)
    {
        return string.Equals(reference.BrowseName?.Name, name, StringComparison.Ordinal)
            || string.Equals(reference.DisplayName?.Text, name, StringComparison.Ordinal)
            || string.Equals(reference.NodeId.Text, $"i={GetOpcUaWellKnownReferenceTypeId(name)}", StringComparison.Ordinal)
            || string.Equals(reference.NodeId.Text, $"ns=0;i={GetOpcUaWellKnownReferenceTypeId(name)}", StringComparison.Ordinal);
    }

    private static int GetOpcUaWellKnownReferenceTypeId(string name)
        => name switch
        {
            "HasTypeDefinition" => 40,
            "HasSubtype" => 45,
            "HasProperty" => 46,
            "HasComponent" => 47,
            "Organizes" => 35,
            _ => -1
        };

    private static bool LooksLikeOpcUaNodeId(string text)
    {
        var value = text.Trim();
        var identifierStart = value.IndexOf(";i=", StringComparison.Ordinal);
        if (identifierStart >= 0)
        {
            return HasValidOpcUaNamespacePrefix(value[..identifierStart])
                && int.TryParse(value[(identifierStart + 3)..], out _);
        }

        identifierStart = value.IndexOf(";s=", StringComparison.Ordinal);
        if (identifierStart >= 0)
        {
            return HasValidOpcUaNamespacePrefix(value[..identifierStart])
                && value.Length > identifierStart + 3;
        }

        identifierStart = value.IndexOf(";g=", StringComparison.Ordinal);
        if (identifierStart >= 0)
        {
            return HasValidOpcUaNamespacePrefix(value[..identifierStart])
                && Guid.TryParse(value[(identifierStart + 3)..], out _);
        }

        identifierStart = value.IndexOf(";b=", StringComparison.Ordinal);
        if (identifierStart >= 0)
        {
            return HasValidOpcUaNamespacePrefix(value[..identifierStart])
                && value.Length > identifierStart + 3;
        }

        if (value.StartsWith("i=", StringComparison.Ordinal))
        {
            return int.TryParse(value[2..], out _);
        }

        if (value.StartsWith("s=", StringComparison.Ordinal))
        {
            return value.Length > 2;
        }

        if (value.StartsWith("g=", StringComparison.Ordinal))
        {
            return Guid.TryParse(value[2..], out _);
        }

        return value.StartsWith("b=", StringComparison.Ordinal) && value.Length > 2;
    }

    private static bool HasValidOpcUaNamespacePrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return true;
        }

        if (prefix.StartsWith("ns=", StringComparison.Ordinal))
        {
            return int.TryParse(prefix[3..], out var namespaceIndex) && namespaceIndex >= 0;
        }

        if (prefix.StartsWith("nsu=", StringComparison.Ordinal))
        {
            return Uri.TryCreate(prefix[4..], UriKind.Absolute, out _);
        }

        return false;
    }

    private static bool IsFunctionCodeAllowedForRegisterType(int functionCode, ModbusRegisterType registerType)
    {
        return registerType switch
        {
            ModbusRegisterType.Coil => functionCode is 1 or 5 or 15,
            ModbusRegisterType.DiscreteInput => functionCode == 2,
            ModbusRegisterType.InputRegister => functionCode == 4,
            ModbusRegisterType.HoldingRegister => functionCode is 3 or 6 or 16,
            _ => false
        };
    }

    private static bool IsReadFunctionCode(int functionCode)
        => functionCode is 1 or 2 or 3 or 4;

    private static bool IsWriteFunctionCode(int functionCode)
        => functionCode is 5 or 6 or 15 or 16;

    private static bool IsSingleWriteFunctionCode(int functionCode)
        => functionCode is 5 or 6;

    private static bool IsWritableRegisterType(ModbusRegisterType registerType)
        => registerType is ModbusRegisterType.Coil or ModbusRegisterType.HoldingRegister;

    private static int GetMaxRegisterCount(ModbusRegisterType registerType)
        => registerType is ModbusRegisterType.Coil or ModbusRegisterType.DiscreteInput ? 2000 : 125;

    private static bool TryParseModbusAddress(string value, out ModbusRegisterType? registerType, out int zeroBasedAddress)
    {
        registerType = null;
        zeroBasedAddress = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var text = value.Trim();
        var separatorIndex = text.IndexOf(':', StringComparison.Ordinal);
        if (separatorIndex >= 0)
        {
            if (!TryParseModbusRegisterType(text[..separatorIndex], out var parsedRegisterType))
            {
                return false;
            }

            registerType = parsedRegisterType;
            text = text[(separatorIndex + 1)..].Trim();
        }

        if (!int.TryParse(text, out var address) || address < 0)
        {
            return false;
        }

        zeroBasedAddress = registerType.HasValue
            ? ToZeroBasedAddress(registerType.Value, address)
            : address;

        return zeroBasedAddress is >= 0 and <= 65535;
    }

    private static bool TryParseModbusRegisterType(string value, out ModbusRegisterType registerType)
    {
        switch (NormalizeIdentifier(value))
        {
            case "coil":
            case "coils":
                registerType = ModbusRegisterType.Coil;
                return true;
            case "discreteinput":
            case "discreteinputs":
            case "discrete":
                registerType = ModbusRegisterType.DiscreteInput;
                return true;
            case "inputregister":
            case "inputregisters":
                registerType = ModbusRegisterType.InputRegister;
                return true;
            case "holdingregister":
            case "holdingregisters":
            case "register":
            case "registers":
                registerType = ModbusRegisterType.HoldingRegister;
                return true;
            default:
                registerType = default;
                return false;
        }
    }

    private static int ToZeroBasedAddress(ModbusRegisterType registerType, int address)
    {
        return registerType switch
        {
            ModbusRegisterType.Coil when address is >= 1 and <= 99999 => address - 1,
            ModbusRegisterType.DiscreteInput when address is >= 10001 and <= 19999 => address - 10001,
            ModbusRegisterType.DiscreteInput when address is >= 1 and <= 99999 => address - 1,
            ModbusRegisterType.InputRegister when address is >= 30001 and <= 39999 => address - 30001,
            ModbusRegisterType.InputRegister when address is >= 1 and <= 99999 => address - 1,
            ModbusRegisterType.HoldingRegister when address is >= 40001 and <= 49999 => address - 40001,
            ModbusRegisterType.HoldingRegister when address is >= 1 and <= 99999 => address - 1,
            _ => address
        };
    }

    private static void ValidateNonSecretString(
        string? value,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (ContainsSecretTerm(value) || LooksLikeInlineSecret(value))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.ProtocolBindingSecretNotAllowed,
                path,
                "Protocol bindings must only contain non-secret endpoint references, addresses, field paths, decode settings, and metadata."));
        }
    }

    private static void ValidateNonSecretMap(
        IReadOnlyDictionary<string, System.Text.Json.JsonElement> values,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        foreach (var (key, value) in values)
        {
            var childPath = $"{path}.{key}";
            if (ContainsSecretTerm(key) || ContainsSecretTerm(value.ToString()) || LooksLikeInlineSecret(value.ToString()))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.ProtocolBindingSecretNotAllowed,
                    childPath,
                    "Protocol binding extension maps must not contain passwords, tokens, connection strings, authorization headers, or inline credentials."));
                continue;
            }

            ValidateNonSecretJsonElement(value, childPath, diagnostics);
        }
    }

    private static void ValidateNonSecretJsonElement(
        System.Text.Json.JsonElement element,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        switch (element.ValueKind)
        {
            case System.Text.Json.JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var childPath = $"{path}.{property.Name}";
                    if (ContainsSecretTerm(property.Name))
                    {
                        diagnostics.Add(new SemanticValidationDiagnostic(
                            SemanticValidationSeverity.Error,
                            SemanticValidationCodes.ProtocolBindingSecretNotAllowed,
                            childPath,
                            "Protocol binding extension maps must not contain passwords, tokens, connection strings, authorization headers, or inline credentials."));
                        continue;
                    }

                    ValidateNonSecretJsonElement(property.Value, childPath, diagnostics);
                }

                break;

            case System.Text.Json.JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    ValidateNonSecretJsonElement(item, $"{path}[{index}]", diagnostics);
                    index++;
                }

                break;

            case System.Text.Json.JsonValueKind.String:
                ValidateNonSecretString(element.GetString(), path, diagnostics);
                break;
        }
    }

    private static bool IsParentPathPrefix(IReadOnlyList<string> parentPath, IReadOnlyList<string> childPath)
    {
        if (parentPath.Count == 0 || childPath.Count <= parentPath.Count)
        {
            return false;
        }

        for (var index = 0; index < parentPath.Count; index++)
        {
            if (!string.Equals(parentPath[index], childPath[index], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidAssetPathSegment(string? segment)
    {
        if (string.IsNullOrWhiteSpace(segment) || segment != segment.Trim())
        {
            return false;
        }

        foreach (var character in segment)
        {
            if ((character >= 'a' && character <= 'z')
                || (character >= '0' && character <= '9')
                || character is '.' or '_' or '-')
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private static string? TryGetMetadataString(
        IReadOnlyDictionary<string, System.Text.Json.JsonElement> metadata,
        string key)
        => metadata.TryGetValue(key, out var value) && value.ValueKind == System.Text.Json.JsonValueKind.String
            ? value.GetString()
            : null;

    private static void ValidateExternalReferenceMetadata(
        IReadOnlyDictionary<string, System.Text.Json.JsonElement> metadata,
        string path,
        ICollection<SemanticValidationDiagnostic> diagnostics)
    {
        foreach (var (key, value) in metadata)
        {
            if (ContainsLiveStateTerm(key) || ContainsLiveStateTerm(value.ToString()))
            {
                diagnostics.Add(new SemanticValidationDiagnostic(
                    SemanticValidationSeverity.Error,
                    SemanticValidationCodes.AssetExternalReferenceStateNotAllowed,
                    $"{path}.{key}",
                    "external reference metadata must stay opaque and must not copy live device state, telemetry, attributes, events, or alarms."));
            }
        }
    }

    private static bool ContainsLiveStateTerm(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] terms =
        [
            "telemetry",
            "attribute",
            "event",
            "alarm",
            "status",
            "state",
            "online",
            "offline",
            "lastvalue",
            "currentvalue"
        ];

        return terms.Any(term => value.Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsSecretTerm(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = NormalizeIdentifier(value);
        string[] terms =
        [
            "password",
            "passwd",
            "pwd",
            "secret",
            "token",
            "accesstoken",
            "refreshtoken",
            "apikey",
            "accesskey",
            "secretkey",
            "sharedaccesskey",
            "connectionstring",
            "credential",
            "credentials",
            "authorization"
        ];

        return terms.Any(term => normalized.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeInlineSecret(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            return true;
        }

        var normalized = NormalizeIdentifier(value);
        string[] fragments =
        [
            "password=",
            "pwd=",
            "token=",
            "access_token=",
            "apikey=",
            "api_key=",
            "secret=",
            "sharedaccesskey=",
            "accountkey=",
            "authorization=",
            "bearer ",
            "basic "
        ];

        return fragments.Any(fragment => normalized.Contains(NormalizeIdentifier(fragment), StringComparison.OrdinalIgnoreCase))
            || value.StartsWith("sk-", StringComparison.Ordinal);
    }

    private static string NormalizeIdentifier(string value)
    {
        var characters = value
            .Where(character => char.IsLetterOrDigit(character) || character is '=' or ' ')
            .Select(char.ToLowerInvariant);

        return string.Concat(characters);
    }

    private static void ValidateRequired(
        ICollection<SemanticValidationDiagnostic> diagnostics,
        string? value,
        string path,
        string code,
        string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                code,
                path,
                message));
        }
    }
}

/// <summary>
/// A single Semantic Core validation diagnostic.
/// </summary>
public sealed record SemanticValidationDiagnostic(
    SemanticValidationSeverity Severity,
    string Code,
    string Path,
    string Message);

/// <summary>
/// Semantic Core validation severity.
/// </summary>
public enum SemanticValidationSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Stable validation diagnostic codes for Semantic Core v1.
/// </summary>
public static class SemanticValidationCodes
{
    public const string ModelRequired = "semantic.model.required";
    public const string ModelSchemaVersionRequired = "semantic.model.schema_version.required";
    public const string ModelIdRequired = "semantic.model.id.required";
    public const string ModelNameRequired = "semantic.model.name.required";
    public const string AssetIdRequired = "semantic.asset.id.required";
    public const string AssetIdDuplicate = "semantic.asset.id.duplicate";
    public const string AssetNameRequired = "semantic.asset.name.required";
    public const string AssetPathRequired = "semantic.asset.path.required";
    public const string AssetPathSegmentInvalid = "semantic.asset.path.segment_invalid";
    public const string AssetPathDuplicate = "semantic.asset.path.duplicate";
    public const string AssetParentMissing = "semantic.asset.parent.missing";
    public const string AssetParentSelf = "semantic.asset.parent.self";
    public const string AssetParentCycle = "semantic.asset.parent.cycle";
    public const string AssetParentPathMismatch = "semantic.asset.parent.path_mismatch";
    public const string AssetPointMissing = "semantic.asset.point.missing";
    public const string AssetPointOwnershipMismatch = "semantic.asset.point.ownership_mismatch";
    public const string AssetExternalReferenceTypeRequired = "semantic.asset.external_reference.type.required";
    public const string AssetExternalReferenceIdRequired = "semantic.asset.external_reference.id.required";
    public const string AssetExternalReferenceUriInvalid = "semantic.asset.external_reference.uri.invalid";
    public const string AssetExternalReferenceStateNotAllowed = "semantic.asset.external_reference.state.not_allowed";
    public const string ProtocolBindingIdRequired = "semantic.protocol_binding.id.required";
    public const string ProtocolBindingIdDuplicate = "semantic.protocol_binding.id.duplicate";
    public const string ProtocolBindingAddressRequired = "semantic.protocol_binding.address.required";
    public const string ProtocolBindingUnused = "semantic.protocol_binding.unused";
    public const string ProtocolBindingSecretNotAllowed = "semantic.protocol_binding.secret.not_allowed";
    public const string ProtocolBindingModbusMetadataRequired = "semantic.protocol_binding.modbus.metadata.required";
    public const string ProtocolBindingModbusKindMismatch = "semantic.protocol_binding.modbus.kind_mismatch";
    public const string ProtocolBindingModbusFunctionCodeInvalid = "semantic.protocol_binding.modbus.function_code.invalid";
    public const string ProtocolBindingModbusAddressInvalid = "semantic.protocol_binding.modbus.address.invalid";
    public const string ProtocolBindingModbusAddressMismatch = "semantic.protocol_binding.modbus.address.mismatch";
    public const string ProtocolBindingModbusUnitIdInvalid = "semantic.protocol_binding.modbus.unit_id.invalid";
    public const string ProtocolBindingModbusRegisterCountInvalid = "semantic.protocol_binding.modbus.register_count.invalid";
    public const string ProtocolBindingModbusAccessInvalid = "semantic.protocol_binding.modbus.access.invalid";
    public const string ProtocolBindingMqttKindMismatch = "semantic.protocol_binding.mqtt.kind_mismatch";
    public const string ProtocolBindingMqttTopicInvalid = "semantic.protocol_binding.mqtt.topic.invalid";
    public const string ProtocolBindingMqttTopicMismatch = "semantic.protocol_binding.mqtt.topic.mismatch";
    public const string ProtocolBindingMqttUnsTopicMismatch = "semantic.protocol_binding.mqtt.uns_topic.mismatch";
    public const string ProtocolBindingMqttQosInvalid = "semantic.protocol_binding.mqtt.qos.invalid";
    public const string ProtocolBindingMqttValueFieldRequired = "semantic.protocol_binding.mqtt.value_field.required";
    public const string ProtocolBindingMqttSparkplugProfileRequired = "semantic.protocol_binding.mqtt.sparkplug_profile.required";
    public const string ProtocolBindingOpcUaMetadataRequired = "semantic.protocol_binding.opcua.metadata.required";
    public const string ProtocolBindingOpcUaKindMismatch = "semantic.protocol_binding.opcua.kind_mismatch";
    public const string ProtocolBindingOpcUaNodeIdRequired = "semantic.protocol_binding.opcua.node_id.required";
    public const string ProtocolBindingOpcUaNodeIdInvalid = "semantic.protocol_binding.opcua.node_id.invalid";
    public const string ProtocolBindingOpcUaNodeIdMismatch = "semantic.protocol_binding.opcua.node_id.mismatch";
    public const string ProtocolBindingOpcUaBrowsePathRequired = "semantic.protocol_binding.opcua.browse_path.required";
    public const string ProtocolBindingOpcUaBrowseNameRequired = "semantic.protocol_binding.opcua.browse_name.required";
    public const string ProtocolBindingOpcUaDataTypeRequired = "semantic.protocol_binding.opcua.data_type.required";
    public const string ProtocolBindingOpcUaEngineeringUnitsRequired = "semantic.protocol_binding.opcua.engineering_units.required";
    public const string ProtocolBindingOpcUaReferencesRequired = "semantic.protocol_binding.opcua.references.required";
    public const string ProtocolBindingOpcUaReferenceTypeRequired = "semantic.protocol_binding.opcua.reference_type.required";
    public const string ProtocolBindingOpcUaReferenceTargetRequired = "semantic.protocol_binding.opcua.reference_target.required";
    public const string ProtocolBindingOpcUaTypeDefinitionReferenceRequired = "semantic.protocol_binding.opcua.type_definition_reference.required";
    public const string ProtocolBindingOpcUaObjectTypeRequired = "semantic.protocol_binding.opcua.object_type.required";
    public const string ProtocolBindingOpcUaVariableTypeRequired = "semantic.protocol_binding.opcua.variable_type.required";
    public const string SemanticPointIdRequired = "semantic.point.semantic_id.required";
    public const string SemanticPointIdDuplicate = "semantic.point.semantic_id.duplicate";
    public const string SemanticPointNameRequired = "semantic.point.name.required";
    public const string SemanticPointQuantityKindRequired = "semantic.point.quantity_kind.required";
    public const string SemanticPointUnitRequired = "semantic.point.unit.required";
    public const string SemanticPointAssetRequired = "semantic.point.asset.required";
    public const string SemanticPointAssetMissing = "semantic.point.asset.missing";
    public const string SemanticPointAssetMembershipMissing = "semantic.point.asset.membership_missing";
    public const string SemanticPointSourceRequired = "semantic.point.source.required";
    public const string SemanticPointSourceBindingMissing = "semantic.point.source.binding_missing";
    public const string SemanticPointWritablePhysicalQuantity = "semantic.point.writable_physical_quantity";
    public const string ProcessGraphIdRequired = "semantic.process_graph.id.required";
    public const string ProcessGraphNameRequired = "semantic.process_graph.name.required";
    public const string ProcessNodeIdRequired = "semantic.process_graph.node.id.required";
    public const string ProcessNodeIdDuplicate = "semantic.process_graph.node.id.duplicate";
    public const string ProcessNodeNameRequired = "semantic.process_graph.node.name.required";
    public const string ProcessNodeTypeRequired = "semantic.process_graph.node.type.required";
    public const string ProcessNodeAssetRequired = "semantic.process_graph.node.asset.required";
    public const string ProcessNodeAssetMissing = "semantic.process_graph.node.asset_missing";
    public const string ProcessNodePointReferenceRequired = "semantic.process_graph.node.point_reference.required";
    public const string ProcessNodeSemanticPointRequired = "semantic.process_graph.node.semantic_point.required";
    public const string ProcessNodeSemanticPointDuplicate = "semantic.process_graph.node.semantic_point.duplicate";
    public const string ProcessNodeSemanticPointMissing = "semantic.process_graph.node.semantic_point.missing";
    public const string ProcessNodeDisconnected = "semantic.process_graph.node.disconnected";
    public const string ProcessEdgeIdRequired = "semantic.process_graph.edge.id.required";
    public const string ProcessEdgeIdDuplicate = "semantic.process_graph.edge.id.duplicate";
    public const string ProcessEdgeNodeRequired = "semantic.process_graph.edge.node.required";
    public const string ProcessEdgeNodeMissing = "semantic.process_graph.edge.node.missing";
    public const string ProcessEdgeSelfReference = "semantic.process_graph.edge.self_reference";
    public const string ProcessEdgeRelationInvalid = "semantic.process_graph.edge.relation.invalid";
    public const string ProcessGraphCycle = "semantic.process_graph.edge.cycle";
    public const string DerivedPointSemanticIdRequired = "semantic.process_graph.derived_point.semantic_id.required";
    public const string DerivedPointSemanticIdDuplicate = "semantic.process_graph.derived_point.semantic_id.duplicate";
    public const string DerivedPointNameRequired = "semantic.process_graph.derived_point.name.required";
    public const string DerivedPointExpressionRequired = "semantic.process_graph.derived_point.expression.required";
    public const string DerivedPointExpressionInvalid = "semantic.process_graph.derived_point.expression.invalid";
    public const string DerivedPointExpressionSemanticReferenceRequired = "semantic.process_graph.derived_point.expression.semantic_reference.required";
    public const string DerivedPointExpressionDependencyMismatch = "semantic.process_graph.derived_point.expression.dependency_mismatch";
    public const string DerivedPointDependencyRequired = "semantic.process_graph.derived_point.dependency.required";
    public const string DerivedPointDependencyDuplicate = "semantic.process_graph.derived_point.dependency.duplicate";
    public const string DerivedPointDependencyMissing = "semantic.process_graph.derived_point.dependency.missing";
    public const string DerivedPointQuantityKindRequired = "semantic.process_graph.derived_point.quantity_kind.required";
    public const string DerivedPointUnitRequired = "semantic.process_graph.derived_point.unit.required";
    public const string DerivedPointUnitIncompatible = "semantic.process_graph.derived_point.unit.incompatible";
    public const string DerivedPointRefreshPolicyRequired = "semantic.process_graph.derived_point.refresh_policy.required";
    public const string DerivedPointRefreshPolicyInvalid = "semantic.process_graph.derived_point.refresh_policy.invalid";
    public const string DerivedPointRefreshIntervalRequired = "semantic.process_graph.derived_point.refresh_interval.required";
    public const string DerivedPointRefreshIntervalInvalid = "semantic.process_graph.derived_point.refresh_interval.invalid";
    public const string DerivedPointCycle = "semantic.process_graph.derived_point.cycle";
    public const string StateModelIdRequired = "semantic.process_graph.state_model.id.required";
    public const string StateModelIdDuplicate = "semantic.process_graph.state_model.id.duplicate";
    public const string StateModelNameRequired = "semantic.process_graph.state_model.name.required";
    public const string StateModelAssetMissing = "semantic.process_graph.state_model.asset.missing";
    public const string StateModelNodeMissing = "semantic.process_graph.state_model.node.missing";
    public const string StateModelStateRequired = "semantic.process_graph.state_model.state.required";
    public const string StateModelDefaultStateRequired = "semantic.process_graph.state_model.default_state.required";
    public const string StateModelDefaultStateMissing = "semantic.process_graph.state_model.default_state.missing";
    public const string StateModelMutualExclusionViolation = "semantic.process_graph.state_model.mutual_exclusion.violation";
    public const string StateDefinitionIdRequired = "semantic.process_graph.state_model.state.id.required";
    public const string StateDefinitionIdDuplicate = "semantic.process_graph.state_model.state.id.duplicate";
    public const string StateDefinitionNameRequired = "semantic.process_graph.state_model.state.name.required";
    public const string StateDefinitionKindInvalid = "semantic.process_graph.state_model.state.kind.invalid";
    public const string StateDefinitionConditionRequired = "semantic.process_graph.state_model.state.condition.required";
    public const string StateDefinitionConditionInvalid = "semantic.process_graph.state_model.state.condition.invalid";
    public const string StateDefinitionConditionReferenceRequired = "semantic.process_graph.state_model.state.condition.reference.required";
    public const string StateDefinitionSemanticReferenceMissing = "semantic.process_graph.state_model.state.semantic_reference.missing";
    public const string StateDefinitionNodeReferenceMissing = "semantic.process_graph.state_model.state.node_reference.missing";
    public const string StateDefinitionDependencyRequired = "semantic.process_graph.state_model.state.dependency.required";
    public const string StateDefinitionDependencyDuplicate = "semantic.process_graph.state_model.state.dependency.duplicate";
    public const string StateDefinitionDependencyMismatch = "semantic.process_graph.state_model.state.dependency.mismatch";
    public const string StateTransitionStateRequired = "semantic.process_graph.state_model.transition.state.required";
    public const string StateTransitionStateMissing = "semantic.process_graph.state_model.transition.state.missing";
    public const string AlarmIdRequired = "semantic.process_graph.alarm.id.required";
    public const string AlarmIdDuplicate = "semantic.process_graph.alarm.id.duplicate";
    public const string AlarmNameRequired = "semantic.process_graph.alarm.name.required";
    public const string AlarmBusinessMeaningRequired = "semantic.process_graph.alarm.business_meaning.required";
    public const string AlarmSeverityRequired = "semantic.process_graph.alarm.severity.required";
    public const string AlarmConditionRequired = "semantic.process_graph.alarm.condition.required";
    public const string AlarmConditionInvalid = "semantic.process_graph.alarm.condition.invalid";
    public const string AlarmConditionReferenceRequired = "semantic.process_graph.alarm.condition.reference.required";
    public const string AlarmDurationInvalid = "semantic.process_graph.alarm.duration.invalid";
    public const string AlarmDependencyRequired = "semantic.process_graph.alarm.dependency.required";
    public const string AlarmDependencyDuplicate = "semantic.process_graph.alarm.dependency.duplicate";
    public const string AlarmDependencyMismatch = "semantic.process_graph.alarm.dependency.mismatch";
    public const string AlarmSemanticReferenceMissing = "semantic.process_graph.alarm.semantic_reference.missing";
    public const string AlarmNodeReferenceMissing = "semantic.process_graph.alarm.node_reference.missing";
    public const string AlarmStateModelReferenceMissing = "semantic.process_graph.alarm.state_model_reference.missing";
    public const string ControlPolicyIdRequired = "semantic.process_graph.control_policy.id.required";
    public const string ControlPolicyIdDuplicate = "semantic.process_graph.control_policy.id.duplicate";
    public const string ControlPolicyNameRequired = "semantic.process_graph.control_policy.name.required";
    public const string ControlPolicyTargetRequired = "semantic.process_graph.control_policy.target.required";
    public const string ControlPolicyTargetDuplicate = "semantic.process_graph.control_policy.target.duplicate";
    public const string ControlPolicyTargetMissing = "semantic.process_graph.control_policy.target.missing";
    public const string ControlPolicyReadPointNotAllowed = "semantic.process_graph.control_policy.read_point.not_allowed";
    public const string ControlPolicyWritablePointPolicyRequired = "semantic.process_graph.control_policy.writable_point.policy.required";
    public const string ControlPolicyRiskInvalid = "semantic.process_graph.control_policy.risk.invalid";
    public const string ControlPolicyApprovalRequired = "semantic.process_graph.control_policy.approval.required";
    public const string ControlPolicyAiOperationModeInvalid = "semantic.process_graph.control_policy.ai_operation_mode.invalid";
    public const string ControlPolicyAutomaticExecutionNotAllowed = "semantic.process_graph.control_policy.automatic_execution.not_allowed";
}
