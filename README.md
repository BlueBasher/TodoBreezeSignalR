The BreezeJs todo sample extended with SignalR.

This todo samples uses SignalR to notify all clients of data-changes.
These clients then look in their breeze local cache to determine if they have the modified entity in cache. If so the refresh this entity.