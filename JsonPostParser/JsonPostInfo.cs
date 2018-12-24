// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var welcome = Welcome.FromJson(jsonString);

namespace QuickType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public struct PostDetails
    {
        public string Src;
        public string PostShortCode;
        public Dimensions PostDimensions;
        public bool IsVideo;
        public string InstagramTypeName;
        public bool isAlbumPost;
        public List<StickyNode> AlbumNodes;

    }
    public partial class JsonPostInfo
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("graphql")]
        public Graphql Graphql { get; set; }
    }
    public partial class Graphql
    {
        [JsonProperty("shortcode_media")]
        public ShortcodeMedia ShortcodeMedia { get; set; }
    }

    public enum Instagram__Typename
    {
        GraphImage,
        GraphVideo,
        GraphSidecar

    }
    public partial class ShortcodeMedia
    {
        [JsonProperty("caption_is_edited")]
        public bool CaptionIsEdited { get; set; }

        [JsonProperty("comments_disabled")]
        public bool CommentsDisabled { get; set; }

        [JsonProperty("dimensions")]
        public Dimensions Dimensions { get; set; }

        [JsonProperty("display_resources")]
        public List<DisplayResource> DisplayResources { get; set; }

        [JsonProperty("display_url")]
        public string DisplayUrl { get; set; }

        [JsonProperty("edge_media_preview_like")]
        public EdgeMediaPreviewLike EdgeMediaPreviewLike { get; set; }



        [JsonProperty("edge_media_to_comment")]
        public EdgeMediaToComment EdgeMediaToComment { get; set; }


        [JsonProperty("edge_web_media_to_related_media")]
        public EdgeWebMediaToRelatedMedia EdgeWebMediaToRelatedMedia { get; set; }

        [JsonProperty("gating_info")]
        public object GatingInfo { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("is_ad")]
        public bool IsAd { get; set; }

        [JsonProperty("is_video")]
        public bool IsVideo { get; set; }

        [JsonProperty("location")]
        public object Location { get; set; }

        [JsonProperty("media_preview")]
        public string MediaPreview { get; set; }


        [JsonProperty("shortcode")]
        public string Shortcode { get; set; }

        [JsonProperty("should_log_client_event")]
        public bool ShouldLogClientEvent { get; set; }

        [JsonProperty("taken_at_timestamp")]
        public long TakenAtTimestamp { get; set; }

        [JsonProperty("tracking_token")]
        public string TrackingToken { get; set; }

        [JsonProperty("__typename")]
        public string Typename { get; set; }

        [JsonProperty("video_url")]
        public string VideoUrl { get; set; }


        [JsonProperty("viewer_has_liked")]
        public bool ViewerHasLiked { get; set; }

        [JsonProperty("viewer_has_saved")]
        public bool ViewerHasSaved { get; set; }

        [JsonProperty("viewer_has_saved_to_collection")]
        public bool ViewerHasSavedToCollection { get; set; }

  

        [JsonProperty("edge_sidecar_to_children")]
        public EdgeSidecarToChildren EdgeSidecarToChildren { get; set; }
    }

    public partial class Dimensions
    {
        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }
    }

    public partial class DisplayResource
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("config_width")]
        public long ConfigWidth { get; set; }

        [JsonProperty("config_height")]
        public long ConfigHeight { get; set; }
    }

    public partial class EdgeMediaPreviewLike
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("edges")]
        public EdgeMediaPreviewLikeEdge[] Edges { get; set; }
    }

    public partial class EdgeMediaPreviewLikeEdge
    {
        [JsonProperty("node")]
        public NodeOwner Node { get; set; }
    }

    public partial class NodeOwner
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("profile_pic_url")]
        public string ProfilePicUrl { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }

    public partial class EdgeMediaTo
    {
        [JsonProperty("edges")]
        public EdgeMediaToCaptionEdge[] Edges { get; set; }
    }

    public partial class EdgeMediaToCaptionEdge
    {
        [JsonProperty("node")]
        public PurpleNode Node { get; set; }
    }

    public partial class PurpleNode
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public partial class EdgeMediaToComment
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("page_info")]
        public PageInfo PageInfo { get; set; }

        [JsonProperty("edges")]
        public EdgeMediaToCommentEdge[] Edges { get; set; }
    }

    public partial class EdgeMediaToCommentEdge
    {
        [JsonProperty("node")]
        public FluffyNode Node { get; set; }
    }

    public partial class EdgeSidecarToChildren
    {
        [JsonProperty("edges")]
        public EdgeSidecarToChildrenEdge[] Edges { get; set; }
    }
    public partial class FluffyNode
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonProperty("owner")]
        public NodeOwner Owner { get; set; }
    }

    public partial class PageInfo
    {
        [JsonProperty("has_next_page")]
        public bool HasNextPage { get; set; }

        [JsonProperty("end_cursor")]
        public string EndCursor { get; set; }
    }

    public partial class EdgeMediaToTaggedUser
    {
        [JsonProperty("edges")]
        public EdgeMediaToTaggedUserEdge[] Edges { get; set; }
    }

    public partial class EdgeMediaToTaggedUserEdge
    {
        [JsonProperty("node")]
        public TentacledNode Node { get; set; }
    }

    public partial class TentacledNode
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }

    public partial class User
    {
        [JsonProperty("biography")]
        public string Biography { get; set; }

        [JsonProperty("blocked_by_viewer")]
        public bool BlockedByViewer { get; set; }

        [JsonProperty("connected_fb_page")]
        public object ConnectedFbPage { get; set; }

        [JsonProperty("country_block")]
        public bool CountryBlock { get; set; }

        [JsonProperty("external_url")]
        public string ExternalUrl { get; set; }

        [JsonProperty("external_url_linkshimmed")]
        public string ExternalUrlLinkshimmed { get; set; }


        [JsonProperty("followed_by_viewer")]
        public bool FollowedByViewer { get; set; }


        [JsonProperty("follows_viewer")]
        public bool FollowsViewer { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("has_blocked_viewer")]
        public bool HasBlockedViewer { get; set; }

        [JsonProperty("has_requested_viewer")]
        public bool HasRequestedViewer { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("is_private")]
        public bool IsPrivate { get; set; }

        [JsonProperty("is_verified")]
        public bool IsVerified { get; set; }


        [JsonProperty("profile_pic_url")]
        public string ProfilePicUrl { get; set; }

        [JsonProperty("profile_pic_url_hd")]
        public string ProfilePicUrlHd { get; set; }

        [JsonProperty("requested_by_viewer")]
        public bool RequestedByViewer { get; set; }


        [JsonProperty("username")]
        public string Username { get; set; }
    }

  

    public partial class EdgeSidecarToChildrenEdge
    {
        [JsonProperty("node")]
        public StickyNode Node { get; set; }
    }

    
    public partial class StickyNode
    {
        [JsonProperty("__typename")]
        public string  Typename { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("shortcode")]
        public string Shortcode { get; set; }

        [JsonProperty("dimensions")]
        public Dimensions Dimensions { get; set; }

        [JsonProperty("gating_info")]
        public object GatingInfo { get; set; }

        [JsonProperty("media_preview")]
        public string MediaPreview { get; set; }

        [JsonProperty("display_url")]
        public string DisplayUrl { get; set; }

        [JsonProperty("display_resources")]
        public DisplayResource[] DisplayResources { get; set; }

      
        [JsonProperty("video_url")]
        public string VideoUrl { get; set; }

        [JsonProperty("video_view_count")]
        public long VideoViewCount { get; set; }

        [JsonProperty("is_video")]
        public bool IsVideo { get; set; }

        [JsonProperty("should_log_client_event")]
        public bool ShouldLogClientEvent { get; set; }

        [JsonProperty("tracking_token")]
        public string TrackingToken { get; set; }

       
       
    }

    public partial class EdgeWebMediaToRelatedMedia
    {
        [JsonProperty("edges")]
        public EdgeWebMediaToRelatedMediaEdge[] Edges { get; set; }
    }

    public partial class EdgeWebMediaToRelatedMediaEdge
    {
        [JsonProperty("node")]
        public IndigoNode Node { get; set; }
    }

    public partial class IndigoNode
    {
        [JsonProperty("shortcode")]
        public string Shortcode { get; set; }

        [JsonProperty("thumbnail_src")]
        public string ThumbnailSrc { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("has_public_page")]
        public bool HasPublicPage { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
    }

    public partial class Owner
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("profile_pic_url")]
        public string ProfilePicUrl { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("blocked_by_viewer")]
        public bool BlockedByViewer { get; set; }

        [JsonProperty("followed_by_viewer")]
        public bool FollowedByViewer { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("has_blocked_viewer")]
        public bool HasBlockedViewer { get; set; }

        [JsonProperty("is_private")]
        public bool IsPrivate { get; set; }

        [JsonProperty("is_unpublished")]
        public bool IsUnpublished { get; set; }

        [JsonProperty("is_verified")]
        public bool IsVerified { get; set; }

        [JsonProperty("requested_by_viewer")]
        public bool RequestedByViewer { get; set; }
    }

    public partial class Welcome
    {
        public static Welcome FromJson(string json) => JsonConvert.DeserializeObject<Welcome>(json, QuickType.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Welcome self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
    }

    internal class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
