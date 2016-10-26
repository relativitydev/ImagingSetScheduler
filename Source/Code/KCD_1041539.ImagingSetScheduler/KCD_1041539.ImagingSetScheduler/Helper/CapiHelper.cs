using System;
using System.Net;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class CapiHelper
	{
		public static bool SubmitRunImagingSetExternal(Int32 workspaceArtifactId, Int32 imagingSetArtifactId, Int32 userArtifactId, string token, bool lockImagesForQc)
		{
			if (token == null)
			{
				throw new ArgumentNullException("token");
			}

			if (workspaceArtifactId < 1)
			{
				throw new ArgumentException(Constant.ErrorMessages.WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
			}

			if (imagingSetArtifactId < 1)
			{
				throw new ArgumentException(Constant.ErrorMessages.IMAGING_SET_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
			}

			if (userArtifactId < 1)
			{
				throw new ArgumentException(Constant.ErrorMessages.USER_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
			}

			string errorContext = String.Format(
				"{0} [WorkspaceArtifactId={1}], ImagingSetArtifactId={2}, UserArtifactId={3}]",
				Constant.ErrorMessages.IMAGING_SET_ERROR,
				workspaceArtifactId,
				imagingSetArtifactId,
				userArtifactId);

			try
			{
				Boolean retVal = SubmitHttpsOrHttpApiRequest(workspaceArtifactId, imagingSetArtifactId, userArtifactId, token, lockImagesForQc);

				return retVal;
			}
			catch (Exception ex)
			{
				throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
			}
		}

		private static bool SubmitHttpsOrHttpApiRequest(int workspaceArtifactId, int imagingSetArtifactId, int userArtifactId, string token, bool lockImagesForQc)
		{
			Boolean retVal;

			//try HTTPS request first
			try
			{
				retVal = SubmitApiRequest(workspaceArtifactId, imagingSetArtifactId, userArtifactId, token, lockImagesForQc, true);
			}
			catch (Exception)
			{
				//If HTTPS request fails, try HTTP request
				retVal = SubmitApiRequest(workspaceArtifactId, imagingSetArtifactId, userArtifactId, token, lockImagesForQc, false);
			}
			return retVal;
		}

		private static bool SubmitApiRequest(int workspaceArtifactId, int imagingSetArtifactId, int userArtifactId, string token, bool lockImagesForQc, Boolean isHttps)
		{
			var runImagingSetExternalActionUrl = String.Format(
				@"{0}://{1}/Relativity/CustomPages/{2}/api/Imaging/RunImagingSetExternal",
				isHttps ? "https" : "http",
				Environment.MachineName,
				Constant.Guids.Application.IMAGING);

			var url = String.Format(
				@"{0}?workspaceArtifactId={1}&imagingSetArtifactId={2}&userArtifactId={3}&authenticationToken={4}&qcEnabled={5}",
				runImagingSetExternalActionUrl,
				workspaceArtifactId,
				imagingSetArtifactId,
				userArtifactId,
				token,
				lockImagesForQc);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			request.ContentLength = 0;

			if (isHttps)
			{
				//ServerCertificateValidationCallback has to be set for HTTPS requests
				System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
			}

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			Boolean retVal;
			if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK)
			{
				retVal = true;
			}
			else
			{
				retVal = false;
			}

			return retVal;
		}
	}
}
