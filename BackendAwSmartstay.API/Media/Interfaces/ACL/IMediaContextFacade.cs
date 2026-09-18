namespace BackendAwSmartstay.API.Media.Interfaces.ACL;

/// <summary>Anti-corruption layer facade of the Media context (where the images of the hotels live).</summary>
public interface IMediaContextFacade
{
    /// <summary>
    ///     True when <paramref name="imageUrl"/> may be used as the image of a hotel: an image of the project's media
    ///     library. When the media library is not configured (local development) any URL is accepted, because images
    ///     cannot be uploaded there anyway.
    /// </summary>
    bool IsAcceptedHotelImageUrl(string imageUrl);

    /// <summary>The part every accepted image URL starts with, for messages; null when any URL is accepted.</summary>
    string? AcceptedHotelImageUrlPrefix { get; }
}
