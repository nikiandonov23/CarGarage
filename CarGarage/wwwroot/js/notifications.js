(() => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationsHub')
        .withAutomaticReconnect()
        .build();

    // Store connection globally for FloatingChatWidget to access
    window.signalRConnection = connection;

    connection.on('UnreadCountUpdated', (count) => {
        const badge = document.querySelector('.messages-unread-badge');

        if (!badge) {
            return;
        }

        badge.textContent = count;

        badge.style.display =
            count > 0
                ? 'inline-block'
                : 'none';
    });

    connection.on('NewMessage', (message) => {
        console.log('SignalR: New message received', message);
        // FloatingChatWidget will handle this through the registered listener
    });

    connection.onreconnecting(() => {
        console.log('SignalR reconnecting...');
    });

    connection.onreconnected(() => {
        console.log('SignalR connected again.');
    });

    connection.onclose(() => {
        console.log('SignalR connection closed.');
    });

    connection.start()
        .then(() => {
            console.log('SignalR connected.');
        })
        .catch(err => {
            console.error(
                'SignalR connection error:',
                err
            );
        });
})();
