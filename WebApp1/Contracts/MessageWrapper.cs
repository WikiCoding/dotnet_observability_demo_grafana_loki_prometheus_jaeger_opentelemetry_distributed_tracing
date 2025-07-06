namespace Contracts;

public record MessageWrapper(IEnumerable<WeatherForecast> wf, string msgEnricher);