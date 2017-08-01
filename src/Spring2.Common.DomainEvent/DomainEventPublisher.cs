using System;
using System.Collections.Generic;
using System.Text;
using Amqp;
using Amqp.Framing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Spring2.Common.DomainEvent {
    public class DomainEventPublisher : DomainEventComms, IDomainEventPublisher {
	
	public DomainEventPublisher(ServiceBusSettings settings, ILogger<DomainEventComms> logger) 
	    : base(settings, logger) { }

	public async Task SendAsync<T>(T @event) where T : class {
	    var data = JsonConvert.SerializeObject(@event);
	    var eventType = @event.GetType().FullName;
	    var address = Settings.Address + @event.GetType().Name;

	    var session = CreateSession();
	    var sender = new SenderLink(session, Settings.AppName, address);
	    var message = new Message(data) {
		ApplicationProperties = new ApplicationProperties(),
		Properties = new Properties {
		    MessageId = Guid.NewGuid().ToString(),
		    GroupId = eventType
		}
	    };
	    message.ApplicationProperties[MESSAGE_TYPE_KEY] = eventType;

	    try {
		await sender.SendAsync(message);
	    } finally {
		await sender.CloseAsync(0);
	    }
	}
    }
}
