﻿using GoogleApi.Entities.Common;
using GoogleApi.Entities.Common.Converters;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Places.Common;
using GoogleApi.Entities.Places.Common.Enums;
using GoogleApi.Entities.Places.Photos.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Shared.Common
{
    public class SearchedPlaceModel
    {
        public PlacesPhotosResponse? Image { get; set; }

        /// <summary>
        /// FormattedAddress is a string containing the human-readable address of this place.
        /// Often this address is equivalent to the "postal address".
        /// The formatted_address property is only returned for a Text Search.
        /// </summary>
        [JsonProperty("formatted_address")]
        public virtual string FormattedAddress { get; set; } = "";

        /// <summary>
        /// Icon contains the URL of a recommended icon which may be displayed to the user when indicating this result.
        /// </summary>
        [JsonProperty("icon")]
        public virtual string IconUrl { get; set; } = "";

        /// <summary>
        /// Geometry contains geometry information about the result, generally including the location (geocode) of the place and (optionally)
        /// the viewport identifying its general area of coverage.
        /// </summary>
        [JsonProperty("geometry")]
        public virtual Geometry Geometry { get; set; }

        /// <summary>
        /// Name contains the human-readable name for the returned result.
        /// For establishment results, this is usually the business name.
        /// </summary>
        [JsonProperty("name")]
        public virtual string Name { get; set; } = "";

        /// <summary>
        /// OpeningHours may contain the following information:
        /// </summary>
        [JsonProperty("opening_hours")]
        public virtual OpeningHours OpeningHours { get; set; }

        /// <summary>
        /// Photos — an array of photo objects, each containing a reference to an image.
        /// A Place Search will return at most one photo object. Performing a Place Details request on the place may return up to ten photos.
        /// More information about Place Photos and how you can use the images in your application can be found in the Place Photos documentation.
        /// </summary>
        [JsonProperty("photos")]
        public virtual IEnumerable<Photo> Photos { get; set; }

        /// <summary>
        /// PlaceId — a textual identifier that uniquely identifies a place.
        /// To retrieve information about the place, pass this identifier in the placeId field of a Places API request. For more information about place IDs, see the place ID overview.
        /// </summary>
        [JsonProperty("place_id")]
        public virtual string PlaceId { get; set; } = "";

        /// <summary>
        /// price_level — The price level of the place, on a scale of 0 to 4.
        /// The exact amount indicated by a specific value will vary from region to region.
        /// </summary>
        [JsonProperty("price_level")]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual PriceLevel? PriceLevel { get; set; }

        /// <summary>
        /// Business Status.
        /// </summary>
        [JsonProperty("business_status")]
        public virtual BusinessStatus BusinessStatus { get; set; }

        /// <summary>
        /// Rating contains the place's rating, from 1.0 to 5.0, based on aggregated user reviews.
        /// </summary>
        [JsonProperty("rating")]
        public virtual double Rating { get; set; }

        /// <summary>
        /// The total number of user ratings.
        /// </summary>
        [JsonProperty("user_ratings_total")]
        public virtual int UserRatingsTotal { get; set; }

        /// <summary>
        /// Types contains an array of feature types describing the given result. See the list of supported types for more information.
        /// XML responses include multiple type elements if more than one type is assigned to the result.
        /// </summary>
        [JsonProperty("types", ItemConverterType = typeof(StringEnumOrDefaultConverter<PlaceLocationType>))]
        public virtual IEnumerable<PlaceLocationType?> Types { get; set; }
    }
}
