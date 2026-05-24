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

        var bindingIds = model.ProtocolBindings
            .Select(binding => binding.BindingId)
            .Where(bindingId => !string.IsNullOrWhiteSpace(bindingId))
            .ToHashSet(StringComparer.Ordinal);

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
            ValidateSemanticPoint(model.SemanticPoints[index], index, assetIds, bindingIds, seenSemanticIds, pointOwners, diagnostics);
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
        ISet<string> bindingIds,
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
        else if (bindingIds.Count > 0 && !bindingIds.Contains(point.Source.BindingId))
        {
            diagnostics.Add(new SemanticValidationDiagnostic(
                SemanticValidationSeverity.Error,
                SemanticValidationCodes.SemanticPointSourceBindingMissing,
                $"{path}.source.bindingId",
                $"source binding '{point.Source.BindingId}' does not exist in protocolBindings."));
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
