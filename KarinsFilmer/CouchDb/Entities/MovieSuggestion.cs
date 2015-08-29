namespace KarinsFilmer.CouchDb.Entities
{
    public class MovieSuggestion : MovieInformationRow
    {
        public double SuggestionWieght { get; }

        public MovieSuggestion(MovieInformationRow source, double suggestionWieght)
        {
            StdDeviation = source.StdDeviation;
            Count = source.Count;
            Mean = source.Mean;
            MovieTitle = source.MovieTitle;
            MovieYear = source.MovieYear;
            ImdbId = source.ImdbId;

            SuggestionWieght = suggestionWieght;
        }
    }
}