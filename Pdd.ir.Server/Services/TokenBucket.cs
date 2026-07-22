namespace Pdd.ir.Server.Services
{
    /// <summary>
    /// Token Bucket Rate Limiter — atomic, thread-safe, production-grade.
    /// Capacity: 5 tokens, Refill: 1 token/second.
    /// </summary>
    public class TokenBucket
    {
        private readonly object _lock = new();
        private double _tokens;
        private DateTime _lastRefill;
        private readonly int _capacity;
        private readonly double _refillRate; // tokens per second

        public TokenBucket(int capacity = 5, double refillRate = 1.0)
        {
            _capacity = capacity;
            _refillRate = refillRate;
            _tokens = capacity;
            _lastRefill = DateTime.UtcNow;
        }

        /// <summary>
        /// Try to consume 1 token. Returns true if allowed, false if rate limited.
        /// </summary>
        public bool TryConsume()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var elapsed = (now - _lastRefill).TotalSeconds;

                // Refill tokens
                _tokens = Math.Min(_capacity, _tokens + elapsed * _refillRate);
                _lastRefill = now;

                if (_tokens >= 1.0)
                {
                    _tokens -= 1.0;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Check if request would be allowed without consuming.
        /// </summary>
        public bool WouldAllow()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var elapsed = (now - _lastRefill).TotalSeconds;
                var tokens = Math.Min(_capacity, _tokens + elapsed * _refillRate);
                return tokens >= 1.0;
            }
        }

        public double AvailableTokens
        {
            get
            {
                lock (_lock)
                {
                    var elapsed = (DateTime.UtcNow - _lastRefill).TotalSeconds;
                    return Math.Min(_capacity, _tokens + elapsed * _refillRate);
                }
            }
        }
    }
}
