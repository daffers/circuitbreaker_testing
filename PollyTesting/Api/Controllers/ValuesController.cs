using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Api.Controllers
{
    using Polly;
    using Polly.CircuitBreaker;

    //[Authorize]
    public class ValuesController : ApiController
    {
        private int _numberOfCalls;
        private ValueProvider _valueProvider;

        public ValuesController()
        {
            _numberOfCalls = 1;
            _valueProvider = new ValueProvider();
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            CircuitBreakerPolicy policy = Policy
                .Handle<TimeoutException>()
                .CircuitBreaker(2, TimeSpan.FromSeconds(10), (exception, timeSpan) => {
                }, () => {
                });

            PolicyResult<IEnumerable<Stuff>> result = policy.ExecuteAndCapture(() => _valueProvider.GetStuff());

            if (result.Outcome == OutcomeType.Failure)
                return "it broke";

            return string.Join(",", result.Result.Select(x => x.AboutTheStuff));
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }

    public class ValueProvider
    {
        private static int _counter = 0;

        public ValueProvider()
        {
        }

        public IEnumerable<Stuff> GetStuff()
        {
            _counter++;
            if (_counter > 1)
                throw new TimeoutException();

            return new List<Stuff>() {new Stuff() {AboutTheStuff = "Yo"}, new Stuff() {AboutTheStuff = "what"} };
        } 
        
    }

    public class Stuff
    {
        public string AboutTheStuff { get; set; }
    }

    public class CircuitBreakerRegister
    {
        private Dictionary<string, CircuitBreakerPolicy> _policies; 

        public CircuitBreakerRegister()
        {
            _policies = new Dictionary<string, CircuitBreakerPolicy>();
        }

        public CircuitBreakerPolicy GetPolicy<TExceptionType>(string breakerName) where TExceptionType : Exception
        {
            if (!_policies.ContainsKey(breakerName))
                _policies.Add(breakerName, BuildDefaultPolicy<TExceptionType>());
            return null;
        }
        
        private CircuitBreakerPolicy BuildDefaultPolicy<TExceptionType>() where TExceptionType : Exception
        {
            return Policy
                .Handle<TExceptionType>()
                .CircuitBreaker(2, TimeSpan.FromSeconds(10), 
                (exception, timeSpan) => {}, 
                () => {}
                );
        }

        public IEnumerable<KeyValuePair<string, CircuitBreakerPolicy>> GetAllBreakers()
        {
            return _policies;
        }
    }
}
