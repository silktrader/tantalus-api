namespace Tantalus.Models; 

public sealed record StatsOverviewResponse {

    public Weight WeightOverview { get; init; }
    
    public sealed record Weight {
        public int LastWeight { get; init; }
        public DateTime LastMeasured { get; init; }
        public int Measurements { get; init; }
        public int MinWeight { get; init; }
        public int MaxWeight { get; init; }
        public int AverageWeight { get; init; }
    }

}

