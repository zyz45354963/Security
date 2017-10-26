using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Microsoft.AspNetCore.Authentication.Test
{
    // REVIEW: Also to go to some testing or shared-source assembly
    // This can only collect from EventSources created AFTER this listener is created
    public class CollectingEventListener : EventListener
    {
        private ConcurrentQueue<EventWrittenEventArgs> _events = new ConcurrentQueue<EventWrittenEventArgs>();
        private HashSet<string> _eventSources;

        public IReadOnlyList<EventWrittenEventArgs> EventsWritten => _events.ToArray();

        public CollectingEventListener(params string[] eventSourceNames)
        {
            _eventSources = new HashSet<string>(eventSourceNames, StringComparer.Ordinal);
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if(_eventSources != null && _eventSources.Contains(eventSource.Name))
            {
                EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            _events.Enqueue(eventData);
        }
    }

    public static class EventWrittenEventArgsExtensions
    {
        public static IDictionary<string, object> GetPayloadAsDictionary(this EventWrittenEventArgs self) =>
            Enumerable.Zip(self.PayloadNames, self.Payload, (name, value) => (name, value)).ToDictionary(t => t.name, t => t.value);
    }
}
