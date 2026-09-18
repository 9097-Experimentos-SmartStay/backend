using BackendAwSmartstay.API.Media.Application.Exceptions;
using BackendAwSmartstay.API.Media.Application.OutboundServices;

namespace BackendAwSmartstay.API.Media.Application.Internal.CommandServices;

/// <summary>Authorizes one upload of a hotel image to the media library (the browser sends the file itself).</summary>
public class HotelImageUploadCommandService(IUploadSignatureService uploadSignatureService)
{
    /// <summary>A fresh signature for one hotel image upload.</summary>
    /// <exception cref="MediaUploadsNotConfiguredException">No media library credentials here (503).</exception>
    public UploadSignature SignHotelImageUpload()
    {
        if (!uploadSignatureService.IsConfigured)
            throw new MediaUploadsNotConfiguredException();
        return uploadSignatureService.SignHotelImageUpload();
    }
}
