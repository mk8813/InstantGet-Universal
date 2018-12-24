using Newtonsoft.Json;
using QuickType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JsonPostParser
{
    public class FetchResourceUrl
    {
        public async Task<PostDetails> GetPostFromUrl(Uri durl, bool isProfile, bool IsHDDownload)
        {
            PostDetails postDetails=new PostDetails();
           
            try
            {
                JsonPostInfo jsonresult = null;
                //string imgSrc = "";
                //string postIdOrUsername = "";
                //Dimensions postDimensions = null;
                bool IsVideo = false;

                using (var client = new HttpClient())
                {
                    Uri urlll = null;

                    if (!string.IsNullOrEmpty(durl.Query))
                    {
                        urlll = new Uri(durl.ToString()
                              .Remove(durl.ToString().IndexOf(durl.Query), durl.Query.ToString().Length)
                              .Insert(durl.ToString().IndexOf(durl.Query), "?__a=1"));
                    }
                    else
                    {
                        urlll = new Uri(durl.ToString()
                              .Insert(durl.ToString().Length, "?__a=1"));
                    }


                    HttpResponseMessage response = await client.GetAsync(urlll);
                    var json = await response.Content.ReadAsStringAsync();

                    jsonresult = JsonConvert.DeserializeObject<JsonPostInfo>(json);
                    if (jsonresult != null)
                    {
                        if (!isProfile)
                        {
                           IsVideo = jsonresult.Graphql.ShortcodeMedia.IsVideo;

                        }

                        postDetails.PostDimensions = jsonresult.Graphql.ShortcodeMedia.Dimensions;//get width height
                        string postType = jsonresult.Graphql.ShortcodeMedia.Typename.ToString();//post type

                        #region beforeEdit
                        //if (!IsVideo)
                        //{
                        //    if (!isProfile)//not profile requested
                        //    {

                        //        postDetails.PostShortCode = jsonresult.Graphql.ShortcodeMedia.Shortcode;

                        //        if (IsHDDownload)//full hd
                        //        {
                        //            postDetails.Src = jsonresult.Graphql.ShortcodeMedia.DisplayUrl; /* FetchLinksFromSource(htmlsrc, out isVideoLink,IsProfile);//.ToString());*/
                        //        }
                        //        else // sd image
                        //        {
                        //            postDetails.Src = jsonresult.Graphql.ShortcodeMedia.DisplayResources[0].Src;
                        //        }

                        //    }// is profile picture
                        //    else
                        //    {
                        //        postDetails.PostShortCode = jsonresult.User.Username;
                        //        if (IsHDDownload)
                        //        {
                        //            postDetails.Src = GetProfilePictureUrl(jsonresult.User.ProfilePicUrlHd);
                        //        }
                        //        else
                        //        {
                        //            postDetails.Src = jsonresult.User.ProfilePicUrlHd;
                        //        }

                        //    }


                        //}
                        //else // video link
                        //{
                        //    postDetails.PostShortCode = jsonresult.Graphql.ShortcodeMedia.Shortcode;

                        //    postDetails.Src = jsonresult.Graphql.ShortcodeMedia.VideoUrl;

                        //}
                        #endregion beforeEdit


                        postDetails.PostShortCode = jsonresult.Graphql.ShortcodeMedia.Shortcode;

                        if (postType == Instagram__Typename.GraphImage.ToString())//single image post
                        {

                            if (IsHDDownload)//full hd
                            {
                                postDetails.Src = jsonresult.Graphql.ShortcodeMedia.DisplayResources.LastOrDefault().Src;//Displayurl /* FetchLinksFromSource(htmlsrc, out isVideoLink,IsProfile);//.ToString());*/
                            }
                            else // sd image
                            {
                                postDetails.Src = jsonresult.Graphql.ShortcodeMedia.DisplayResources.FirstOrDefault().Src;//[0]
                            }
                        }
                        else if (postType == Instagram__Typename.GraphVideo.ToString())//video post
                        {

                            postDetails.Src = jsonresult.Graphql.ShortcodeMedia.VideoUrl;
                            postDetails.IsVideo = jsonresult.Graphql.ShortcodeMedia.IsVideo;

                        }
                        if (postType == Instagram__Typename.GraphSidecar.ToString())//album  post
                        {
                            //to do
                            if (jsonresult.Graphql.ShortcodeMedia.EdgeSidecarToChildren != null)
                            {
                                List<StickyNode> lstNodes = new List<StickyNode>();
                                foreach (var item in jsonresult.Graphql.ShortcodeMedia.EdgeSidecarToChildren.Edges)
                                {
                                    lstNodes.Add(item.Node);
                                }
                                postDetails.isAlbumPost = true;
                                postDetails.AlbumNodes = lstNodes;
                                //imgSrc = await ShowAlbumListDialog(lstNodes);


                            }
                        }
                        return postDetails;
                    }

                    else
                    {
                        return postDetails;
                    }
                }
            }
            catch (Exception)
            {

                return postDetails;
            }

        }
        private static string GetProfilePictureUrl(string url)
        {
            Regex profileRegex = new Regex(@"s[0-9][0-9][0-9]x[0-9][0-9][0-9]");
            if (profileRegex.IsMatch(url))
            {
                url = profileRegex.Replace(url, "s");
            }
            return url;
        }


    }

   
}
