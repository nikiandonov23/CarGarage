/**
 * Floating Chat Widget - Vanilla JavaScript
 * Handles floating chat bubbles, windows, messaging, and real-time updates
 * 
 * Usage: Initialize with FloatingChatWidget.init() on page load
 */

class FloatingChatWidget {
    constructor() {
        this.container = document.getElementById('floating-chat-container');
        this.bubbles = document.getElementById('chat-bubbles');
        this.windows = document.getElementById('chat-windows');

        this.openChats = new Map(); // Map of senderId -> window element
        this.conversations = new Map(); // Map of conversationId -> messages
        this.signalRConnection = null;

        this.init();
    }

    /**
     * Initialize the widget
     */
    async init() {
        if (!this.container) {
            console.warn('Floating chat container not found');
            return;
        }

        // Load unread conversations
        await this.loadUnreadConversations();

        // Setup SignalR listeners if available
        if (typeof signalR !== 'undefined') {
            this.setupSignalR();
        }

        console.log('Floating Chat Widget initialized');
    }

    /**
     * Load unread conversations from API
     */
    async loadUnreadConversations() {
        try {
            const response = await fetch('/api/messages/unread-conversations', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                console.warn('Failed to load unread conversations:', response.status);
                return;
            }

            const conversations = await response.json();

            // Clear existing bubbles
            this.bubbles.innerHTML = '';

            // Create bubble for each conversation
            conversations.forEach(conv => {
                this.createChatBubble(conv);
            });

            console.log('Loaded', conversations.length, 'unread conversations');
        } catch (error) {
            console.error('Error loading unread conversations:', error);
        }
    }

    /**
     * Create a chat bubble (avatar button)
     */
    createChatBubble(conversation) {
        const template = document.getElementById('chat-bubble-template');
        const bubble = template.content.cloneNode(true);

        const bubbleDiv = bubble.querySelector('.chat-bubble');
        bubbleDiv.dataset.senderId = conversation.senderId;

        const btn = bubble.querySelector('.chat-bubble-btn');
        btn.title = conversation.senderName || conversation.senderId;

        // Set unread badge
        const badge = bubble.querySelector('.bubble-unread-badge');
        if (conversation.unreadCount > 0) {
            badge.textContent = conversation.unreadCount;
            badge.style.display = 'flex';
        }

        // Add click handler
        btn.addEventListener('click', () => {
            this.toggleChatWindow(conversation);
        });

        this.bubbles.appendChild(bubble);

        // Store conversation
        const convId = conversation.conversationId;
        if (!this.conversations.has(convId)) {
            this.conversations.set(convId, {
                ...conversation,
                localMessages: [...conversation.messages] || []
            });
        }
    }

    /**
     * Toggle chat window open/close
     */
    toggleChatWindow(conversation) {
        const senderId = conversation.senderId;
        const convId = conversation.conversationId;

        if (this.openChats.has(senderId)) {
            // Close window
            this.closeChatWindow(senderId);
        } else {
            // Open window
            this.openChatWindow(conversation);
        }
    }

    /**
     * Open a chat window
     */
    openChatWindow(conversation) {
        const senderId = conversation.senderId;
        const convId = conversation.conversationId;

        // Check if already open
        if (this.openChats.has(senderId)) {
            return;
        }

        const template = document.getElementById('chat-window-template');
        const windowEl = template.content.cloneNode(true);

        const chatWindow = windowEl.querySelector('.chat-window');
        chatWindow.dataset.senderId = senderId;
        chatWindow.dataset.conversationId = convId;

        // Set header title
        const title = windowEl.querySelector('.chat-window-title');
        title.textContent = conversation.senderName || senderId;

        // Close button
        const closeBtn = windowEl.querySelector('.chat-window-close-btn');
        closeBtn.addEventListener('click', () => {
            this.closeChatWindow(senderId);
        });

        // Messages container
        const messagesList = windowEl.querySelector('.chat-messages-list');

        // Render existing messages
        if (conversation.messages && conversation.messages.length > 0) {
            conversation.messages.forEach(msg => {
                this.renderMessage(messagesList, msg);
            });
        } else {
            messagesList.innerHTML = '<div class="chat-empty-state"><div class="chat-empty-state-icon"><i class="bi bi-chat-left"></i></div>Никакви съобщения</div>';
        }

        // Input handling
        const input = windowEl.querySelector('.chat-message-input');
        const sendBtn = windowEl.querySelector('.chat-send-btn');

        const handleSend = async () => {
            const content = input.value.trim();
            if (!content) return;

            input.value = '';
            sendBtn.disabled = true;

            await this.sendMessage(senderId, convId, content);

            sendBtn.disabled = false;
            input.focus();
        };

        sendBtn.addEventListener('click', handleSend);
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                handleSend();
            }
        });

        // Focus handler - mark messages as read
        chatWindow.addEventListener('focus', () => {
            this.markConversationAsRead(convId);
        }, true);

        this.windows.appendChild(windowEl);
        this.openChats.set(senderId, chatWindow);

        // Auto-scroll to bottom
        setTimeout(() => {
            messagesList.scrollTop = messagesList.scrollHeight;
        }, 0);

        // Mark as read
        this.markConversationAsRead(convId);

        console.log('Opened chat with', senderId);
    }

    /**
     * Close a chat window
     */
    closeChatWindow(senderId) {
        const chatWindow = this.openChats.get(senderId);
        if (!chatWindow) return;

        chatWindow.parentElement.removeChild(chatWindow);
        this.openChats.delete(senderId);

        console.log('Closed chat with', senderId);
    }

    /**
     * Render a single message
     */
    renderMessage(container, message, isCurrentUser = false) {
        // Remove empty state if exists
        const emptyState = container.querySelector('.chat-empty-state');
        if (emptyState) {
            emptyState.remove();
        }

        const template = document.getElementById('message-template');
        const msgEl = template.content.cloneNode(true);

        const messageDiv = msgEl.querySelector('.message');
        messageDiv.dataset.messageId = message.id;
        messageDiv.classList.add(isCurrentUser ? 'sent' : 'received');

        const content = msgEl.querySelector('.message-content');
        content.textContent = message.content;

        const time = msgEl.querySelector('.message-time');
        const sentAt = new Date(message.sentAt);
        time.textContent = this.formatTime(sentAt);

        container.appendChild(msgEl);

        // Auto-scroll to bottom
        container.scrollTop = container.scrollHeight;
    }

    /**
     * Send a message
     */
    async sendMessage(receiverId, conversationId, content) {
        try {
            const response = await fetch('/api/messages/send-from-widget', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({
                    receiverId,
                    conversationId,
                    content
                })
            });

            if (!response.ok) {
                console.error('Failed to send message:', response.status);
                return;
            }

            const result = await response.json();

            // Add message to local UI
            const chatWindow = Array.from(this.openChats.values())
                .find(w => w.dataset.senderId === receiverId);

            if (chatWindow) {
                const messagesList = chatWindow.querySelector('.chat-messages-list');
                this.renderMessage(messagesList, {
                    id: result.id,
                    content,
                    sentAt: result.sentAt
                }, true);
            }

            console.log('Message sent successfully');
        } catch (error) {
            console.error('Error sending message:', error);
        }
    }

    /**
     * Mark conversation as read
     */
    async markConversationAsRead(conversationId) {
        try {
            const response = await fetch('/api/messages/mark-conversation-read', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({
                    conversationId
                })
            });

            if (!response.ok) {
                console.warn('Failed to mark conversation as read:', response.status);
                return;
            }

            const result = await response.json();

            // Update unread badge
            const chatWindow = Array.from(this.windows.querySelectorAll('.chat-window'))
                .find(w => w.dataset.conversationId === conversationId);

            if (chatWindow) {
                const senderId = chatWindow.dataset.senderId;
                const badge = this.bubbles.querySelector(
                    `[data-sender-id="${senderId}"] .bubble-unread-badge`
                );
                if (badge) {
                    badge.style.display = 'none';
                    badge.textContent = '0';
                }
            }

            console.log('Marked conversation as read:', conversationId);
        } catch (error) {
            console.error('Error marking conversation as read:', error);
        }
    }

    /**
     * Setup SignalR event listeners
     */
    setupSignalR() {
        try {
            // Check if signalR connection exists in global scope (from notifications.js)
            if (window.signalRConnection && typeof window.signalRConnection.on === 'function') {
                const conn = window.signalRConnection;

                // Listen for new messages
                conn.on('NewMessage', (message) => {
                    this.handleNewMessage(message);
                });

                // Listen for unread count updates
                conn.on('UnreadCountUpdated', (count) => {
                    console.log('Unread count updated:', count);
                });

                console.log('SignalR event listeners registered');
            }
        } catch (error) {
            console.warn('SignalR setup warning:', error);
        }
    }

    /**
     * Handle new incoming message (via SignalR)
     */
    handleNewMessage(message) {
        const senderId = message.senderId;
        const convId = message.conversationId;

        // Check if chat window is open
        const openWindow = this.openChats.get(senderId);

        if (openWindow) {
            // Add to existing open window
            const messagesList = openWindow.querySelector('.chat-messages-list');
            this.renderMessage(messagesList, message, false);
        } else {
            // Create/update bubble with unread badge
            const bubble = this.bubbles.querySelector(`[data-sender-id="${senderId}"]`);
            if (bubble) {
                const badge = bubble.querySelector('.bubble-unread-badge');
                let count = parseInt(badge.textContent) || 0;
                count++;
                badge.textContent = count;
                badge.style.display = 'flex';
            } else {
                // New conversation - reload all conversations
                this.loadUnreadConversations();
            }
        }

        console.log('New message received from', senderId);
    }

    /**
     * Format timestamp
     */
    formatTime(date) {
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) {
            return 'сега';
        } else if (diffMins < 60) {
            return `преди ${diffMins}м`;
        } else if (diffHours < 24) {
            return `преди ${diffHours}ч`;
        } else if (diffDays < 7) {
            return `преди ${diffDays}д`;
        } else {
            return date.toLocaleDateString('bg-BG', {
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
    }

    /**
     * Static initialization method
     */
    static instance = null;

    static init() {
        if (!this.instance) {
            this.instance = new FloatingChatWidget();
        }
        return this.instance;
    }
}

/**
 * Export for use in other modules
 */
if (typeof module !== 'undefined' && module.exports) {
    module.exports = FloatingChatWidget;
}

/**
 * Extend signalR connection if available
 * Store connection globally for FloatingChatWidget to access
 */
(() => {
    // Wait for signalR connection to be established
    const checkSignalR = () => {
        // This is handled by the notifications.js file
        // We'll enhance it there to support floating chat
    };

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            FloatingChatWidget.init();
        });
    } else {
        FloatingChatWidget.init();
    }
})();
