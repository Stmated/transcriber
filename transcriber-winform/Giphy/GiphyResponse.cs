using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace transcriber_winform.Giphy
{
    public class GiphyData
    {
        [JsonProperty]
        public String Type { get; set; }

        [JsonProperty]
        public String Id { get; set; }

        [JsonProperty(PropertyName = "image_original_url")]
        public String ImageOriginalUrl { get; set; }

        [JsonProperty(PropertyName = "image_url")]
        public String ImageUrl { get; set; }

        [JsonProperty(PropertyName = "image_mp4_url")]
        public String ImageMp4Url { get; set; }

        [JsonProperty(PropertyName = "image_width")]
        public int ImageWidth { get; set; }

        [JsonProperty(PropertyName = "image_height")]
        public int ImageHeight { get; set; }

        [JsonProperty]
        public GiphyImages Images { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GiphyImage
    {
        [JsonProperty]
        public String Url { get; set; }

        [JsonProperty]
        public String Mp4 { get; set; }

        [JsonProperty]
        public int Width { get; set; }

        [JsonProperty]
        public int Height { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GiphyImages
    {
        [JsonProperty]
        public GiphyImage Original { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GiphyMeta
    {
        [JsonProperty]
        public String Status { get; set; }

        [JsonProperty]
        public String Message { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GiphyRandomResponse
    {
        [JsonProperty]
        public GiphyData Data { get; set; }

        [JsonProperty]
        public GiphyMeta Meta { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GiphySearchResponse
    {
        [JsonProperty]
        public List<GiphyData> Data { get; set; }

        [JsonProperty]
        public GiphyMeta Meta { get; set; }
    }



    //{
    //    "data": {
    //        type: "gif",
    //        id: "Ggjwvmqktuvf2",
    //        url: "http://giphy.com/gifs/american-psycho-christian-bale-Ggjwvmqktuvf2",
    //        image_original_url: "http://s3.amazonaws.com/giphygifs/media/Ggjwvmqktuvf2/giphy.gif",
    //        image_url: "http://s3.amazonaws.com/giphygifs/media/Ggjwvmqktuvf2/giphy.gif",
    //        image_mp4_url: "http://s3.amazonaws.com/giphygifs/media/Ggjwvmqktuvf2/giphy.mp4",
    //        image_frames: "11",
    //        image_width: "500",
    //        image_height: "256",
    //        fixed_height_downsampled_url: "http://s3.amazonaws.com/giphygifs/media/Ggjwvmqktuvf2/200_d.gif",
    //        fixed_height_downsampled_width: "391",
    //        fixed_height_downsampled_height: "200",
    //        fixed_width_downsampled_url: "http://s3.amazonaws.com/giphygifs/media/Ggjwvmqktuvf2/200w_d.gif",
    //        fixed_width_downsampled_width: "200",
    //        fixed_width_downsampled_height: "102",
    //        fixed_height_small_url: "http://s3.amazonaws.com/giphygifs/media/Ggjwvmqktuvf2/100.gif",
    //        fixed_height_small_still_url: "http://s3.amazonaws.com/giphygifs/media/Ggjwvmqktuvf2/100_s.gif",
    //        fixed_height_small_width: "195",
    //        fixed_height_small_height: "100",
    //        fixed_width_small_url: "http://s3.amazonaws.com/giphygifs/media/Ggjwvmqktuvf2/100w.gif",
    //        fixed_width_small_still_url: "http://s3.amazonaws.com/giphygifs/media/Ggjwvmqktuvf2/100w_s.gif",
    //        fixed_width_small_width: "100",
    //        fixed_width_small_height: "51"                 
    //    },
    //    "meta": {
    //        "status": 200,
    //        "msg": "OK"
    //    }
    //}
}
