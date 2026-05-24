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
        for (var index = 0; index < model.SemanticPoints.Count; index++)
        {
            ValidateSemanticPoint(model.SemanticPoints[index], index, assetIds, assetsById, bindingsById, seenSemanticIds, pointOwners, diagnostics);
        }

        ValidateAssetPointOwnership(model.Assets, pointOwners, diagnostics);

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
}
