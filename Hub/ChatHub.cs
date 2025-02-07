using Microsoft.AspNetCore.SignalR;

namespace Whisp.Hub {

    public class ChatHub(IDictionary<string, UserGroupConnection> connection) : Microsoft.AspNetCore.SignalR.Hub {

        private readonly IDictionary<string, UserGroupConnection> _connection = connection;

        // method for joining a group
        public async Task JoinGroup(UserGroupConnection userConnection){
            // Adds the current user's connection (identified by Context.ConnectionId) to a specified group indicated by userConnection.Group
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.ChatGroup!);

            // updates a dictionary _connection with the user connection information
            _connection[Context.ConnectionId] = userConnection;

            // Notifies all members of the group that a new member has joined.
            await Clients.Group(userConnection.ChatGroup!)
                         .SendAsync("ReceiveMessage", "Whisp", $"{userConnection.User} has joined the group", DateTime.Now);

            // Notifies connected users in the group about the new member.
            await NotifyConnectedUsersInGroup(userConnection.ChatGroup!);
        }

        // Method to send a message
        public async Task SendChatMessage(string message){
            if (_connection.TryGetValue(Context.ConnectionId, out UserGroupConnection? userGroupConnection) && userGroupConnection != null)
            {
                // Checks if the current user's connection ID exists in the _connection dictionary.
                if (userGroupConnection.ChatGroup != null)
                {
                    await Clients.Group(userGroupConnection.ChatGroup)
                                .SendAsync("ReceiveMessage", userGroupConnection.User, message, DateTime.Now);
                    // Sends a message to all clients in the specified chat group.
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connection.TryGetValue(Context.ConnectionId, out UserGroupConnection? groupConnection) && groupConnection != null)
            {
                // Remove user from the group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupConnection.ChatGroup!);
                
                // Remove user from the connections dictionary
                _connection.Remove(Context.ConnectionId);
                
                // Notify group members that user has left
                await Clients.Group(groupConnection.ChatGroup!)
                    .SendAsync("ReceiveMessage", "Whisp", $"{groupConnection.User} has left the group", DateTime.Now);
                    
                // Update the connected users list
                await NotifyConnectedUsersInGroup(groupConnection.ChatGroup!);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public Task NotifyConnectedUsersInGroup(string group)
        {
            // Retrieve a list of connected users in the specified group from the _connection dictionary
            var connectedUsers = _connection.Values
                .Where(connection => connection.ChatGroup == group)
                .Select(connection => connection.User);

            // Send an update message to all clients in the specified chat group with the list of connected users
            return Clients.Group(group).SendAsync("ConnectedUser", connectedUsers);
        }
    }
}