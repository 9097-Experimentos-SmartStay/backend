namespace BackendAwSmartstay.API.Media.Interfaces.REST.Resources;

/// <summary>
///     A signed authorization to upload ONE image straight to Cloudinary. Post the file as multipart to
///     <paramref name="UploadUrl"/> with the fields <c>file</c>, <c>api_key</c>, <c>timestamp</c>, <c>signature</c>,
///     <c>upload_preset</c> and <c>folder</c> (these exact values; no Authorization header) and use the
///     <c>secure_url</c> of the answer as the hotel's <c>imageUrl</c>. Valid for one hour.
/// </summary>
/// <param name="CloudName">Cloudinary cloud name.</param>
/// <param name="ApiKey">Public API key (field <c>api_key</c>).</param>
/// <param name="Timestamp">Unix seconds (field <c>timestamp</c>).</param>
/// <param name="Signature">SHA-1 signature of the parameters (field <c>signature</c>).</param>
/// <param name="UploadPreset">Signed upload preset (field <c>upload_preset</c>).</param>
/// <param name="Folder">Destination folder (field <c>folder</c>).</param>
/// <param name="UploadUrl">Upload API endpoint.</param>
public record UploadSignatureResource(
    string CloudName,
    string ApiKey,
    long Timestamp,
    string Signature,
    string UploadPreset,
    string Folder,
    string UploadUrl);
