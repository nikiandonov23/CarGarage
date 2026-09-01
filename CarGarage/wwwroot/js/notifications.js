(() => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/notificationsHub')
        .withAutomaticReconnect()
        .build();

    connection.on('UnreadCountUpdated', (count) => {
        const badge = document.querySelector('.messages-unread-badge');
        if (!badge) return;
        badge.textContent = count;
        badge.style.display = count > 0 ? 'inline-block' : 'none';
    });

    connection.start().catch(err => console.error(err.toString()));
})();
