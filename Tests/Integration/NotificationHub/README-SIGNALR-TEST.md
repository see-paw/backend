# SignalR Notifications Test

Simple HTML page to test SignalR real-time notifications.

## Prerequisites

- Backend API running on `https://localhost:5001`
- Python 3.x installed (for serving the HTML file)

## Setup

1. **Start the Backend API**
```bash
   cd C:\LDS Project\backend\WebAPI
   dotnet run
```
   
   Confirm it's running on: `https://localhost:5001`

2. **Start the HTTP Server**
   
   Open a new terminal in the same dir as `signalr-test.html`:
```bash
   cd C:\LDS Project
   python -m http.server 8080
```

3. **Open in Browser**
   
   Navigate to: **`http://localhost:8080/signalr-test.html`**
   
   ⚠️ **Important:** Do NOT open the file directly (file://). Use the HTTP server URL!

## Usage

### Test Credentials

- **User 1:** `carlos.notif@test.com` / `Pa$$w0rd`
- **User 2:** `alice.notif@test.com` / `Pa$$w0rd`

### Testing Flow

1. Click **"Connect"** to establish SignalR connection
2. Use **Postman** or another user to trigger ownership requests
3. Notifications will appear in real-time on the page

### Example: Trigger Notification via Postman

**Create Ownership Request:**
```
POST https://localhost:5001/api/ownershiprequests
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "animalID": "f055cc31-fdeb-4c65-bb73-4f558f67dd5d"
}
```

**Update Ownership Status (Shelter Admin):**
```
PUT https://localhost:5001/api/ownershiprequests/analysing/{ownershipRequestId}
Authorization: Bearer {shelter-admin-token}
```

### Or Use the Built-in Button

Click **"Create Test Ownership"** to automatically create a test request.

## Troubleshooting

### "Login failed! Check credentials."
- Verify the API is running on the correct port
- Check `const API_URL` in the HTML file matches your API URL

### CORS Errors
- Make sure you're accessing via `http://localhost:8080`, NOT `file://`
- Verify CORS is configured in `Program.cs` to allow `http://localhost:8080`

### Connection Fails
- Confirm the backend is running
- Check browser console (F12) for detailed error messages
- Verify the NotificationHub is mapped in `Program.cs`

## Notes

- This is a **manual testing tool**, not an automated test
- Notifications are user-specific (only the logged-in user receives their notifications)
- Connection is maintained until you click "Disconnect" or close the browser