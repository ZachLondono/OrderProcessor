namespace ApplicationCore.Infrastructure;

class CacheConfiguration {

    public bool UseLocalCache { get; set; }

    public int MaxCacheSize { get; set; }

    public int TimeAlive { get; set; }

}
