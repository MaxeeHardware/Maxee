﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var maxeeDivisions = MaxeeDivisions.FromJson(jsonString);
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Maxee.DemoAPIConsole.MaxeeDivisionsQuickType
{

    public partial class MaxeeDivisions
    {
        [JsonProperty("data")]
        public Datum[] Data { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("aggregateResults")]
        public object AggregateResults { get; set; }

        [JsonProperty("errors")]
        public object Errors { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("companyId")]
        public long CompanyId { get; set; }
    }

    public partial class MaxeeDivisions
    {
        public static MaxeeDivisions FromJson(string json) => JsonConvert.DeserializeObject<MaxeeDivisions>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this MaxeeDivisions self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
