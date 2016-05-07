using System.Xml.Serialization;

namespace AnnotateMovieDirectories.GoodReads
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class GoodreadsReponse
    {
        [XmlElement]
        public GrRequest Request { get; set; }
        [XmlElement("search")]
        public GrQuery Query { get; set; }
    }
    [XmlType(AnonymousType = true)]
    public class GrRequest
    {
        [XmlElement("authentication")]
        public bool Authentication { get; set; }

        [XmlElement("key")]
        public string Key { get; set; }

        [XmlElement("method")]
        public string Method { get; set; }
    }

    public class GrQuery
    {
        public string Query { get; set; }
        [XmlElement("results-start")]
        public int ResultsStart { get; set; }
        [XmlElement("results-end")]
        public int ResultsEnd { get; set; }
        [XmlElement("total-results")]
        public int TotalResults { get; set; }
        [XmlElement("source")]
        public string Source { get; set; }
        [XmlElement("query-time-seconds")]
        public decimal QueryTimeSeconds { get; set; }
        [XmlElement("results")]
        [XmlArrayItem("work", IsNullable = true)]
        public GrWork[] Results { get; set; }
    }

    public class GrWork
    {
        [XmlElement("id")]
        public GrField Id { get; set; }
        [XmlElement("books_count")]
        public GrField BooksCount { get; set; }
        [XmlElement("ratings_count")]
        public GrField RatingsCount { get; set; }
        [XmlElement("text_reviews_count")]
        public GrField TextReviewsCount { get; set; }
        [XmlElement("original_publication_year")]
        public GrField OriginalPublicationYear { get; set; }
        [XmlElement("original_publication_month")]
        public GrField OriginalPublicationMonth { get; set; }
        [XmlElement("original_publication_day")]
        public GrField OriginalPublicationDay { get; set; }
        [XmlElement("average_rating")]
        public GrField AverageRating { get; set; }
        [XmlElement("best_book")]
        public GrBestBook Book { get; set; }

    }

    public class GrBestBook
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlElement("id")]
        public GrField Id { get; set; }
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("author")]
        public GrAuthor Author { get; set; }
        [XmlElement("image_url")]
        public string ImageUrl { get; set; }
        [XmlElement("small_image_url")]
        public string SmallImageUrl { get; set; }

    }

    public class GrAuthor
    {
        [XmlElement("id")]
        public GrField Id { get; set; }
        [XmlElement("name")]
        public string Name { get; set; }
    }

    public class GrField
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute]
        public bool nil { get; set; }

        public string Value { get; set; }
    }
}