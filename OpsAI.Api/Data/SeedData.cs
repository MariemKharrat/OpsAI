using Microsoft.Azure.Cosmos;
using OpsAI.Api.Models;

namespace OpsAI.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(CosmosClient client, string databaseName)
    {
        var db = client.GetDatabase(databaseName);
        var ticketsContainer = db.GetContainer("Tickets");
        var knowledgeContainer = db.GetContainer("KnowledgeArticles");
        var auditContainer = db.GetContainer("AuditEntries");

        // Check if already seeded
        var countQuery = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
        var iterator = ticketsContainer.GetItemQueryIterator<int>(countQuery);
        var response = await iterator.ReadNextAsync();
        if (response.FirstOrDefault() > 0) return;

        // Seed tickets
        foreach (var ticket in GetSeedTickets())
        {
            await ticketsContainer.CreateItemAsync(ticket, new PartitionKey(ticket.PartitionKey));
        }

        // Seed knowledge articles
        foreach (var article in GetSeedArticles())
        {
            await knowledgeContainer.CreateItemAsync(article, new PartitionKey(article.PartitionKey));
        }

        // Seed audit entries for some tickets
        foreach (var entry in GetSeedAuditEntries())
        {
            await auditContainer.CreateItemAsync(entry, new PartitionKey(entry.PartitionKey));
        }
    }

    private static List<Ticket> GetSeedTickets()
    {
        var baseDate = DateTime.UtcNow.AddDays(-14);
        return
        [
            new()
            {
                Id = "tkt-001", TicketNumber = "INC-2024-001",
                Subject = "Cannot connect to VPN from home office",
                Description = "I've been unable to connect to the corporate VPN since this morning. I get an error 'Connection timed out' after entering my credentials. I've tried restarting my laptop and router but the issue persists. I need VPN access to reach internal tools for a client deadline today.",
                Status = TicketStatus.Open, Priority = TicketPriority.High, Category = TicketCategory.VPN,
                RequesterName = "Sarah Chen", RequesterEmail = "sarah.chen@company.com", RequesterDepartment = "Engineering",
                Assignee = "Mike Torres", Tags = ["vpn", "remote-work", "urgent"],
                CreatedAt = baseDate.AddHours(2), UpdatedAt = baseDate.AddHours(3)
            },
            new()
            {
                Id = "tkt-002", TicketNumber = "INC-2024-002",
                Subject = "Password reset request - locked out of account",
                Description = "My account has been locked after too many failed login attempts. I think my password expired over the weekend. Please reset my password so I can access my workstation and email. My employee ID is EMP-4521.",
                Status = TicketStatus.InProgress, Priority = TicketPriority.Medium, Category = TicketCategory.AccountManagement,
                RequesterName = "James Wilson", RequesterEmail = "james.wilson@company.com", RequesterDepartment = "Sales",
                Assignee = "Lisa Park", Tags = ["password", "account-lock", "active-directory"],
                CreatedAt = baseDate.AddHours(5), UpdatedAt = baseDate.AddHours(6)
            },
            new()
            {
                Id = "tkt-003", TicketNumber = "INC-2024-003",
                Subject = "New laptop setup for incoming team member",
                Description = "We have a new hire starting next Monday (David Park, Engineering team). Need a MacBook Pro 16\" configured with standard dev tools: VS Code, Docker Desktop, Node.js, Python 3.11, Git, and access to our GitHub org. Also need Office 365 license and Slack access.",
                Status = TicketStatus.New, Priority = TicketPriority.Medium, Category = TicketCategory.Hardware,
                RequesterName = "Rachel Adams", RequesterEmail = "rachel.adams@company.com", RequesterDepartment = "Engineering",
                Tags = ["onboarding", "hardware", "new-hire"],
                CreatedAt = baseDate.AddDays(1), UpdatedAt = baseDate.AddDays(1)
            },
            new()
            {
                Id = "tkt-004", TicketNumber = "INC-2024-004",
                Subject = "Outlook keeps crashing when opening attachments",
                Description = "For the past 3 days, Outlook crashes every time I try to open a PDF attachment. Other file types seem to work fine. I'm running Outlook 365 on Windows 11. I've already tried the repair option in Control Panel but it didn't help. This is blocking me from reviewing contracts.",
                Status = TicketStatus.Open, Priority = TicketPriority.High, Category = TicketCategory.Email,
                RequesterName = "Maria Lopez", RequesterEmail = "maria.lopez@company.com", RequesterDepartment = "Legal",
                Assignee = "Tom Richards", Tags = ["outlook", "crash", "pdf", "office365"],
                CreatedAt = baseDate.AddDays(1).AddHours(4), UpdatedAt = baseDate.AddDays(2)
            },
            new()
            {
                Id = "tkt-005", TicketNumber = "INC-2024-005",
                Subject = "Request access to Azure DevOps project 'Phoenix'",
                Description = "I've recently transferred to the Platform team and need contributor access to the 'Phoenix' project in Azure DevOps. My manager (Kevin Zhao) has approved this. I need access to repos, pipelines, and boards.",
                Status = TicketStatus.Pending, Priority = TicketPriority.Low, Category = TicketCategory.AccessPermissions,
                RequesterName = "Alex Turner", RequesterEmail = "alex.turner@company.com", RequesterDepartment = "Platform",
                Assignee = "Lisa Park", Tags = ["access", "azure-devops", "permissions"],
                CreatedAt = baseDate.AddDays(2), UpdatedAt = baseDate.AddDays(2).AddHours(3)
            },
            new()
            {
                Id = "tkt-006", TicketNumber = "INC-2024-006",
                Subject = "Printer on 3rd floor not printing - paper jam",
                Description = "The HP LaserJet on the 3rd floor (near the kitchen) has a paper jam that I can't clear. The display shows 'Paper Jam in Tray 2' but I don't see any paper stuck. Multiple people on this floor are affected.",
                Status = TicketStatus.Open, Priority = TicketPriority.Medium, Category = TicketCategory.Printing,
                RequesterName = "David Kim", RequesterEmail = "david.kim@company.com", RequesterDepartment = "Marketing",
                Assignee = "Tom Richards", Tags = ["printer", "paper-jam", "hardware"],
                CreatedAt = baseDate.AddDays(2).AddHours(6), UpdatedAt = baseDate.AddDays(2).AddHours(7)
            },
            new()
            {
                Id = "tkt-007", TicketNumber = "INC-2024-007",
                Subject = "Suspicious email received - possible phishing",
                Description = "I received an email from 'IT-Support@c0mpany.com' (note the zero) asking me to verify my credentials by clicking a link. I did NOT click the link but wanted to report it. The subject was 'Urgent: Your account will be deactivated'. Several colleagues received the same email.",
                Status = TicketStatus.InProgress, Priority = TicketPriority.Critical, Category = TicketCategory.Security,
                RequesterName = "Nina Patel", RequesterEmail = "nina.patel@company.com", RequesterDepartment = "Finance",
                Assignee = "Security Team", Tags = ["phishing", "security-incident", "email"],
                CreatedAt = baseDate.AddDays(3), UpdatedAt = baseDate.AddDays(3).AddHours(1)
            },
            new()
            {
                Id = "tkt-008", TicketNumber = "INC-2024-008",
                Subject = "WiFi connectivity dropping intermittently in Building B",
                Description = "Multiple team members in Building B, 2nd floor are experiencing WiFi drops every 15-20 minutes. It reconnects after about 30 seconds but it's disrupting video calls and causing lost work. Started happening after last weekend's maintenance window.",
                Status = TicketStatus.Open, Priority = TicketPriority.High, Category = TicketCategory.Network,
                RequesterName = "Chris Johnson", RequesterEmail = "chris.johnson@company.com", RequesterDepartment = "Product",
                Assignee = "Network Team", Tags = ["wifi", "connectivity", "building-b", "intermittent"],
                CreatedAt = baseDate.AddDays(3).AddHours(4), UpdatedAt = baseDate.AddDays(4)
            },
            new()
            {
                Id = "tkt-009", TicketNumber = "INC-2024-009",
                Subject = "Need Microsoft Visio license for architecture diagrams",
                Description = "I need a Microsoft Visio Professional license for creating system architecture diagrams. This is required for the upcoming platform migration project. Budget code: ENG-2024-Q2. Approved by VP Engineering.",
                Status = TicketStatus.Pending, Priority = TicketPriority.Low, Category = TicketCategory.Software,
                RequesterName = "Omar Hassan", RequesterEmail = "omar.hassan@company.com", RequesterDepartment = "Architecture",
                Tags = ["software-request", "license", "visio"],
                CreatedAt = baseDate.AddDays(4), UpdatedAt = baseDate.AddDays(4).AddHours(2)
            },
            new()
            {
                Id = "tkt-010", TicketNumber = "INC-2024-010",
                Subject = "Monitor flickering on docking station",
                Description = "My external monitor (Dell 27\") has been flickering since I got a new docking station last week. It happens mostly when I have multiple apps open. I've tried different cables (HDMI and DisplayPort) but the issue persists. The monitor works fine when connected directly to the laptop.",
                Status = TicketStatus.New, Priority = TicketPriority.Medium, Category = TicketCategory.Hardware,
                RequesterName = "Emily Watson", RequesterEmail = "emily.watson@company.com", RequesterDepartment = "Design",
                Tags = ["monitor", "docking-station", "flickering", "hardware"],
                CreatedAt = baseDate.AddDays(4).AddHours(6), UpdatedAt = baseDate.AddDays(4).AddHours(6)
            },
            new()
            {
                Id = "tkt-011", TicketNumber = "INC-2024-011",
                Subject = "Cannot access shared drive \\\\fileserver\\projects",
                Description = "Getting 'Access Denied' when trying to open the Projects shared drive. I had access until yesterday. No changes were made to my account that I know of. I need this for the quarterly report due tomorrow.",
                Status = TicketStatus.Open, Priority = TicketPriority.High, Category = TicketCategory.AccessPermissions,
                RequesterName = "Robert Chang", RequesterEmail = "robert.chang@company.com", RequesterDepartment = "Finance",
                Assignee = "Mike Torres", Tags = ["shared-drive", "access-denied", "permissions"],
                CreatedAt = baseDate.AddDays(5), UpdatedAt = baseDate.AddDays(5).AddHours(1)
            },
            new()
            {
                Id = "tkt-012", TicketNumber = "INC-2024-012",
                Subject = "Slack not syncing messages on mobile app",
                Description = "My Slack mobile app (iOS) stopped syncing messages 2 days ago. I can see messages on desktop but mobile shows old messages only. I've tried logging out and back in, clearing cache, and reinstalling the app. Running iOS 17.4.",
                Status = TicketStatus.New, Priority = TicketPriority.Low, Category = TicketCategory.Software,
                RequesterName = "Amy Foster", RequesterEmail = "amy.foster@company.com", RequesterDepartment = "HR",
                Tags = ["slack", "mobile", "sync-issue", "ios"],
                CreatedAt = baseDate.AddDays(5).AddHours(4), UpdatedAt = baseDate.AddDays(5).AddHours(4)
            },
            new()
            {
                Id = "tkt-013", TicketNumber = "INC-2024-013",
                Subject = "Two-factor authentication not working after phone upgrade",
                Description = "I upgraded to a new iPhone and now my MFA codes from Microsoft Authenticator aren't working. I can't log into any corporate systems. I still have my old phone but it's been factory reset. I need this resolved ASAP as I have client meetings today.",
                Status = TicketStatus.InProgress, Priority = TicketPriority.Critical, Category = TicketCategory.Security,
                RequesterName = "Daniel Brooks", RequesterEmail = "daniel.brooks@company.com", RequesterDepartment = "Consulting",
                Assignee = "Lisa Park", Tags = ["mfa", "authenticator", "phone-upgrade", "urgent"],
                CreatedAt = baseDate.AddDays(6), UpdatedAt = baseDate.AddDays(6).AddHours(2)
            },
            new()
            {
                Id = "tkt-014", TicketNumber = "INC-2024-014",
                Subject = "Request to install Docker Desktop on workstation",
                Description = "I need Docker Desktop installed on my Windows workstation for local development. Our team is containerizing our microservices and I need to run them locally for testing. My machine has 32GB RAM and meets the requirements.",
                Status = TicketStatus.Resolved, Priority = TicketPriority.Low, Category = TicketCategory.Software,
                RequesterName = "Kevin Nguyen", RequesterEmail = "kevin.nguyen@company.com", RequesterDepartment = "Engineering",
                Assignee = "Tom Richards", Tags = ["docker", "software-install", "development"],
                CreatedAt = baseDate.AddDays(6).AddHours(5), UpdatedAt = baseDate.AddDays(7), ResolvedAt = baseDate.AddDays(7)
            },
            new()
            {
                Id = "tkt-015", TicketNumber = "INC-2024-015",
                Subject = "Email distribution list not receiving external emails",
                Description = "Our team distribution list (platform-team@company.com) is not receiving emails from external senders. Internal emails work fine. This started after the Exchange migration last week. We're missing important vendor communications.",
                Status = TicketStatus.Open, Priority = TicketPriority.High, Category = TicketCategory.Email,
                RequesterName = "Jessica Martinez", RequesterEmail = "jessica.martinez@company.com", RequesterDepartment = "Platform",
                Assignee = "Mike Torres", Tags = ["distribution-list", "exchange", "external-email"],
                CreatedAt = baseDate.AddDays(7), UpdatedAt = baseDate.AddDays(7).AddHours(3)
            },
            new()
            {
                Id = "tkt-016", TicketNumber = "INC-2024-016",
                Subject = "Laptop battery draining extremely fast",
                Description = "My ThinkPad X1 Carbon battery now lasts only about 2 hours when it used to last 8+. I haven't installed any new software recently. Battery health in settings shows 'Good'. This started about a week ago. I travel frequently and need reliable battery life.",
                Status = TicketStatus.New, Priority = TicketPriority.Medium, Category = TicketCategory.Hardware,
                RequesterName = "Paul Anderson", RequesterEmail = "paul.anderson@company.com", RequesterDepartment = "Sales",
                Tags = ["battery", "laptop", "thinkpad", "performance"],
                CreatedAt = baseDate.AddDays(8), UpdatedAt = baseDate.AddDays(8)
            },
            new()
            {
                Id = "tkt-017", TicketNumber = "INC-2024-017",
                Subject = "VPN split tunneling configuration request",
                Description = "Our team needs split tunneling enabled on VPN so that video calls (Teams/Zoom) don't route through the VPN. The current full-tunnel setup causes terrible call quality for our remote team. This has been discussed with our CISO and approved.",
                Status = TicketStatus.Pending, Priority = TicketPriority.Medium, Category = TicketCategory.VPN,
                RequesterName = "Michelle Lee", RequesterEmail = "michelle.lee@company.com", RequesterDepartment = "Engineering",
                Assignee = "Network Team", Tags = ["vpn", "split-tunnel", "video-calls", "approved"],
                CreatedAt = baseDate.AddDays(8).AddHours(4), UpdatedAt = baseDate.AddDays(9)
            },
            new()
            {
                Id = "tkt-018", TicketNumber = "INC-2024-018",
                Subject = "Bulk user account creation for summer interns",
                Description = "We have 12 summer interns starting June 1st. Need AD accounts, email, Slack access, and badge access for all of them. I'll attach the spreadsheet with names and departments. They should have 'Intern' security group access only.",
                Status = TicketStatus.New, Priority = TicketPriority.Medium, Category = TicketCategory.AccountManagement,
                RequesterName = "Laura Thompson", RequesterEmail = "laura.thompson@company.com", RequesterDepartment = "HR",
                Tags = ["bulk-accounts", "interns", "onboarding", "active-directory"],
                CreatedAt = baseDate.AddDays(9), UpdatedAt = baseDate.AddDays(9)
            },
            new()
            {
                Id = "tkt-019", TicketNumber = "INC-2024-019",
                Subject = "Production server high CPU alert - app-server-03",
                Description = "Received Datadog alert: app-server-03 CPU at 98% for over 15 minutes. The application logs show repeated database connection timeouts. This server handles payment processing. Need immediate investigation.",
                Status = TicketStatus.InProgress, Priority = TicketPriority.Critical, Category = TicketCategory.Network,
                RequesterName = "Systems Alert", RequesterEmail = "alerts@company.com", RequesterDepartment = "Operations",
                Assignee = "Mike Torres", Tags = ["production", "cpu", "alert", "payment", "critical"],
                CreatedAt = baseDate.AddDays(10), UpdatedAt = baseDate.AddDays(10).AddHours(1)
            },
            new()
            {
                Id = "tkt-020", TicketNumber = "INC-2024-020",
                Subject = "Zoom Room setup for new conference room 4B",
                Description = "New conference room 4B needs a Zoom Room setup. Equipment has been delivered (Poly Studio X50, touch controller, 65\" display). Need network drops configured, device enrolled in our Zoom admin, and room added to the booking system.",
                Status = TicketStatus.New, Priority = TicketPriority.Low, Category = TicketCategory.Hardware,
                RequesterName = "Facilities Team", RequesterEmail = "facilities@company.com", RequesterDepartment = "Facilities",
                Tags = ["zoom-room", "conference-room", "setup", "av-equipment"],
                CreatedAt = baseDate.AddDays(10).AddHours(5), UpdatedAt = baseDate.AddDays(10).AddHours(5)
            },
            new()
            {
                Id = "tkt-021", TicketNumber = "INC-2024-021",
                Subject = "Resolved: Software update broke custom Excel macros",
                Description = "After the Office 365 update pushed last night, several of our financial reporting macros stopped working. Getting 'Compile Error' messages. These macros are critical for month-end close.",
                Status = TicketStatus.Resolved, Priority = TicketPriority.High, Category = TicketCategory.Software,
                RequesterName = "Patricia Wells", RequesterEmail = "patricia.wells@company.com", RequesterDepartment = "Finance",
                Assignee = "Tom Richards", Tags = ["excel", "macros", "office-update", "finance"],
                CreatedAt = baseDate.AddDays(11), UpdatedAt = baseDate.AddDays(12), ResolvedAt = baseDate.AddDays(12)
            },
            new()
            {
                Id = "tkt-022", TicketNumber = "INC-2024-022",
                Subject = "BitLocker recovery key needed - laptop won't boot",
                Description = "My laptop is asking for a BitLocker recovery key after a BIOS update. I can't find the key anywhere and I have a presentation in 2 hours. Laptop serial: THK-2024-8847. I've never been given a recovery key.",
                Status = TicketStatus.Open, Priority = TicketPriority.Critical, Category = TicketCategory.Security,
                RequesterName = "Mark Stevens", RequesterEmail = "mark.stevens@company.com", RequesterDepartment = "Executive",
                Assignee = "Lisa Park", Tags = ["bitlocker", "recovery", "urgent", "laptop"],
                CreatedAt = baseDate.AddDays(12), UpdatedAt = baseDate.AddDays(12).AddHours(1)
            },
            new()
            {
                Id = "tkt-023", TicketNumber = "INC-2024-023",
                Subject = "Teams meeting recordings not saving to SharePoint",
                Description = "When I record Teams meetings, they no longer appear in SharePoint/OneDrive. The recording seems to complete successfully but I can't find the files anywhere. This started happening about a week ago. Other team members have the same issue.",
                Status = TicketStatus.New, Priority = TicketPriority.Medium, Category = TicketCategory.Software,
                RequesterName = "Sandra Wright", RequesterEmail = "sandra.wright@company.com", RequesterDepartment = "Training",
                Tags = ["teams", "recording", "sharepoint", "onedrive"],
                CreatedAt = baseDate.AddDays(13), UpdatedAt = baseDate.AddDays(13)
            },
        ];
    }

    private static List<KnowledgeArticle> GetSeedArticles()
    {
        return
        [
            new()
            {
                Id = "kb-001", Title = "How to Reset Your Password",
                Content = "## Password Reset Procedure\n\n### Self-Service Reset\n1. Go to https://passwordreset.company.com\n2. Enter your email address\n3. Complete MFA verification\n4. Create a new password (min 12 chars, 1 uppercase, 1 number, 1 special)\n\n### If Account is Locked\n- Wait 30 minutes for automatic unlock, OR\n- Contact IT Help Desk for immediate unlock\n- Provide your Employee ID for verification\n\n### Password Requirements\n- Minimum 12 characters\n- Cannot reuse last 10 passwords\n- Expires every 90 days\n- Must contain: uppercase, lowercase, number, special character",
                Category = TicketCategory.AccountManagement, Tags = ["password", "reset", "account", "active-directory"],
                ViewCount = 342, HelpfulCount = 287
            },
            new()
            {
                Id = "kb-002", Title = "VPN Connection Troubleshooting Guide",
                Content = "## VPN Troubleshooting Steps\n\n### Common Issues & Fixes\n\n**Connection Timeout:**\n1. Check internet connectivity (try browsing a website)\n2. Restart VPN client\n3. Try alternate VPN server (vpn2.company.com)\n4. Flush DNS: `ipconfig /flushdns` (Windows) or `sudo dscacheutil -flushcache` (Mac)\n\n**Authentication Failed:**\n1. Verify credentials (same as AD login)\n2. Check if MFA token is current\n3. Ensure account is not locked\n\n**Slow Connection:**\n1. Try a closer VPN server\n2. Disconnect/reconnect\n3. Check if split tunneling is enabled\n\n### VPN Servers\n- Primary: vpn.company.com\n- US-East: vpn-east.company.com\n- US-West: vpn-west.company.com\n- EU: vpn-eu.company.com",
                Category = TicketCategory.VPN, Tags = ["vpn", "connection", "troubleshooting", "remote"],
                ViewCount = 518, HelpfulCount = 445
            },
            new()
            {
                Id = "kb-003", Title = "Setting Up Multi-Factor Authentication (MFA)",
                Content = "## MFA Setup Guide\n\n### Initial Setup\n1. Download Microsoft Authenticator from App Store/Google Play\n2. Log into https://mysignins.microsoft.com\n3. Click 'Security Info' > 'Add sign-in method'\n4. Select 'Authenticator app' and follow prompts\n5. Scan the QR code with your phone\n\n### Phone Upgrade/Replacement\n1. **Before** wiping old phone: Export accounts or note recovery codes\n2. Install Authenticator on new phone\n3. Sign in with your Microsoft account to restore\n4. If old phone unavailable: Contact IT for MFA reset (requires identity verification)\n\n### Backup Methods\n- SMS to registered phone number\n- Recovery codes (save these securely!)\n- Hardware security key (FIDO2)",
                Category = TicketCategory.Security, Tags = ["mfa", "authenticator", "security", "two-factor"],
                ViewCount = 298, HelpfulCount = 251
            },
            new()
            {
                Id = "kb-004", Title = "Outlook Troubleshooting - Common Issues",
                Content = "## Outlook Common Issues\n\n### Outlook Crashing\n1. Start Outlook in Safe Mode: `outlook.exe /safe`\n2. Disable add-ins: File > Options > Add-ins > Manage COM Add-ins\n3. Repair Office: Control Panel > Programs > Office > Repair\n4. Clear Outlook cache: `%localappdata%\\Microsoft\\Outlook`\n5. Create new Outlook profile if issues persist\n\n### Attachment Issues\n- **Can't open PDF:** Update Adobe Reader or install it\n- **Blocked attachments:** IT policy may block .exe, .bat files\n- **Large attachments:** Max size is 25MB, use OneDrive for larger files\n\n### Performance Issues\n- Archive old emails (older than 1 year)\n- Reduce mailbox size (limit: 50GB)\n- Disable unnecessary add-ins\n- Compact .ost file",
                Category = TicketCategory.Email, Tags = ["outlook", "email", "crash", "attachments", "office365"],
                ViewCount = 423, HelpfulCount = 356
            },
            new()
            {
                Id = "kb-005", Title = "Printer Setup and Troubleshooting",
                Content = "## Printer Guide\n\n### Adding a Network Printer\n1. Go to Settings > Printers & Scanners\n2. Click 'Add printer'\n3. Select from discovered printers or enter IP:\n   - 3rd Floor: 10.0.3.100 (HP LaserJet)\n   - 2nd Floor: 10.0.2.100 (HP Color)\n   - 1st Floor: 10.0.1.100 (HP LaserJet)\n\n### Paper Jam Resolution\n1. Turn off printer\n2. Open all access panels (front, rear, trays)\n3. Gently pull paper in direction of paper path\n4. Check for small torn pieces\n5. Close panels and restart\n6. If 'Tray 2' error persists: Remove tray completely, check for debris, reseat\n\n### Common Issues\n- **Print queue stuck:** Restart Print Spooler service\n- **Poor quality:** Replace toner/run cleaning cycle\n- **Offline:** Check network cable, restart printer",
                Category = TicketCategory.Printing, Tags = ["printer", "paper-jam", "setup", "troubleshooting"],
                ViewCount = 234, HelpfulCount = 198
            },
            new()
            {
                Id = "kb-006", Title = "WiFi Connectivity Issues",
                Content = "## WiFi Troubleshooting\n\n### Quick Fixes\n1. Forget network and reconnect\n2. Restart WiFi adapter: Disable > Enable in Network Settings\n3. Run network troubleshooter (Windows)\n4. Reset network stack: `netsh winsock reset` (admin cmd)\n\n### Corporate WiFi (CorpNet)\n- SSID: CorpNet-5G (preferred) or CorpNet-2.4G\n- Authentication: 802.1X with AD credentials\n- If prompted for certificate, accept the company root CA\n\n### Known Issues\n- Building B, 2nd floor: AP capacity issues during peak hours\n- Conference rooms: Switch to 5GHz if 2.4GHz is congested\n- After maintenance windows: May need to forget and rejoin\n\n### Guest WiFi\n- SSID: Company-Guest\n- No password required\n- Limited bandwidth, no internal access",
                Category = TicketCategory.Network, Tags = ["wifi", "network", "connectivity", "wireless"],
                ViewCount = 312, HelpfulCount = 267
            },
            new()
            {
                Id = "kb-007", Title = "Software Installation Request Process",
                Content = "## Software Installation Guide\n\n### Self-Service (Software Center)\nPre-approved software available without IT ticket:\n- VS Code, Git, Node.js, Python\n- Slack, Zoom, Teams\n- Adobe Reader, 7-Zip\n- Chrome, Firefox\n\nOpen Software Center from Start Menu > Install desired app\n\n### Requires IT Approval\nSubmit a ticket for:\n- Docker Desktop (requires admin rights)\n- Adobe Creative Suite (license required)\n- Microsoft Visio/Project (license required)\n- Any software not in Software Center\n\n### Request Process\n1. Submit ticket with: Software name, version, business justification\n2. Manager approval (auto-routed)\n3. IT review (security scan if needed)\n4. Installation scheduled or remote install\n\n### Timeline\n- Self-service: Immediate\n- Standard request: 1-3 business days\n- License procurement: 5-10 business days",
                Category = TicketCategory.Software, Tags = ["software", "installation", "request", "license"],
                ViewCount = 445, HelpfulCount = 389
            },
            new()
            {
                Id = "kb-008", Title = "New Employee IT Onboarding Checklist",
                Content = "## IT Onboarding Checklist\n\n### Before Start Date (IT Tasks)\n- [ ] Create AD account\n- [ ] Assign Office 365 license\n- [ ] Create email address\n- [ ] Add to appropriate security groups\n- [ ] Configure hardware (laptop/desktop)\n- [ ] Install standard software\n- [ ] Create badge access\n- [ ] Add to Slack workspace\n\n### Day 1 (Employee)\n- [ ] Sign in and change password\n- [ ] Set up MFA\n- [ ] Configure email on mobile\n- [ ] Join required Slack channels\n- [ ] Test VPN connection\n- [ ] Verify printer access\n\n### Manager Responsibilities\n- Submit onboarding ticket 5+ business days before start\n- Specify: department, role, required access/tools\n- Provide org chart position for group memberships",
                Category = TicketCategory.AccountManagement, Tags = ["onboarding", "new-hire", "checklist", "setup"],
                ViewCount = 267, HelpfulCount = 234
            },
            new()
            {
                Id = "kb-009", Title = "BitLocker Recovery Key Retrieval",
                Content = "## BitLocker Recovery\n\n### When Is Recovery Key Needed?\n- After BIOS/firmware update\n- Hardware changes (RAM, SSD)\n- TPM reset or failure\n- Too many incorrect PIN attempts\n\n### How to Find Your Recovery Key\n1. **Azure AD Portal:** https://myaccount.microsoft.com > Devices > View BitLocker Keys\n2. **IT Help Desk:** Call x4357 with laptop serial number and employee ID\n3. **Self-Service Portal:** https://bitlocker.company.com (requires alternate login)\n\n### Prevention\n- Always suspend BitLocker before BIOS updates\n- Save recovery key to your Azure AD account\n- Keep a printed copy in a secure location\n\n### If Key Cannot Be Found\n- IT can retrieve from Active Directory (if enrolled)\n- Worst case: OS reinstall required (data loss)",
                Category = TicketCategory.Security, Tags = ["bitlocker", "recovery", "encryption", "security"],
                ViewCount = 178, HelpfulCount = 156
            },
            new()
            {
                Id = "kb-010", Title = "Shared Drive and File Server Access",
                Content = "## Shared Drive Access\n\n### Requesting Access\n1. Submit IT ticket with:\n   - Share path (e.g., \\\\fileserver\\projects)\n   - Required permission level (Read/Write/Full)\n   - Business justification\n   - Manager approval\n\n### Troubleshooting Access Denied\n1. Verify you're on the corporate network (or VPN)\n2. Try accessing via IP: \\\\10.0.1.50\\sharename\n3. Clear credential cache: `cmdkey /delete:fileserver`\n4. Re-map drive: disconnect and reconnect\n5. Check group membership: `whoami /groups` in CMD\n\n### Common Shares\n- \\\\fileserver\\projects - Project files\n- \\\\fileserver\\departments - Department folders\n- \\\\fileserver\\shared - Company-wide documents\n\n### Note\nAccess is group-based. Transfers between departments may require new access requests.",
                Category = TicketCategory.AccessPermissions, Tags = ["shared-drive", "file-server", "access", "permissions"],
                ViewCount = 289, HelpfulCount = 245
            },
            new()
            {
                Id = "kb-011", Title = "Reporting Phishing and Suspicious Emails",
                Content = "## Phishing Response Guide\n\n### How to Identify Phishing\n- Sender address doesn't match company domain exactly\n- Urgent language pressuring immediate action\n- Links to unfamiliar URLs (hover to check)\n- Requests for credentials or personal info\n- Unexpected attachments\n\n### What to Do\n1. **DO NOT** click any links or open attachments\n2. **DO NOT** reply to the email\n3. Click 'Report Phishing' button in Outlook ribbon\n4. Forward to security@company.com with headers\n5. If you clicked a link: Immediately change your password and contact IT\n\n### If You Entered Credentials\n1. Change password IMMEDIATELY\n2. Call IT Security: x4358\n3. Enable additional MFA\n4. Monitor your account for suspicious activity\n\n### The Security team will:\n- Block the sender domain\n- Scan for similar emails company-wide\n- Notify affected users\n- Update email filters",
                Category = TicketCategory.Security, Tags = ["phishing", "security", "email", "suspicious"],
                ViewCount = 567, HelpfulCount = 523
            },
            new()
            {
                Id = "kb-012", Title = "Docking Station and Monitor Troubleshooting",
                Content = "## Docking Station Issues\n\n### Monitor Not Detected\n1. Unplug dock from laptop, wait 10 seconds, reconnect\n2. Try a different port on the dock\n3. Check cable connections (both ends)\n4. Update dock firmware: Download from manufacturer site\n5. Update display drivers\n\n### Flickering Display\n1. Change refresh rate: Display Settings > Advanced > 60Hz\n2. Try a different cable (prefer DisplayPort over HDMI)\n3. Update dock firmware\n4. Check for interference (move wireless devices away)\n5. If only on dock: Likely a bandwidth issue - reduce resolution or refresh rate\n\n### Supported Docks\n- ThinkPad USB-C Dock Gen 2 (recommended)\n- Dell WD19TB Thunderbolt\n- CalDigit TS4\n\n### Known Limitations\n- Max 2 external monitors on USB-C docks\n- 4K@60Hz requires Thunderbolt or DP 1.4\n- Some docks need driver installation",
                Category = TicketCategory.Hardware, Tags = ["docking-station", "monitor", "display", "flickering"],
                ViewCount = 198, HelpfulCount = 167
            },
            new()
            {
                Id = "kb-013", Title = "Teams and Zoom Meeting Room Setup",
                Content = "## Meeting Room Technology Guide\n\n### Starting a Meeting\n1. Tap the touch controller to wake\n2. Select 'Join Meeting' and enter meeting ID, OR\n3. Use 'One Touch Join' for calendar-synced meetings\n4. For ad-hoc: Select 'New Meeting'\n\n### Troubleshooting\n- **No display:** Check HDMI input source on TV\n- **No audio:** Verify speaker is selected in room device\n- **Camera not working:** Restart the room device\n- **Can't join:** Check room account is licensed\n\n### Booking Rooms\n- Use Outlook: New Meeting > Add Room > Select room\n- Use Teams: Calendar > New Meeting > Add room\n- Walk-up: Available rooms show green on door panel\n\n### Room Equipment (Standard)\n- Poly Studio X50 (camera + speaker bar)\n- 65\" Display\n- Touch controller\n- Wireless screen sharing (tap NFC tag or use meeting share)",
                Category = TicketCategory.Hardware, Tags = ["teams", "zoom", "meeting-room", "video"],
                ViewCount = 156, HelpfulCount = 134
            },
            new()
            {
                Id = "kb-014", Title = "Laptop Battery Optimization",
                Content = "## Battery Life Optimization\n\n### Quick Wins\n1. Reduce screen brightness (30-50% for indoor use)\n2. Close unnecessary browser tabs\n3. Disable Bluetooth if not in use\n4. Use 'Battery Saver' mode when below 50%\n5. Disconnect external devices when on battery\n\n### Identifying Battery Drain\nRun battery report: `powercfg /batteryreport` (Windows)\n- Check 'Recent Usage' for high-drain periods\n- Look at 'Battery Capacity History' for degradation\n\n### Common Causes of Rapid Drain\n- Background sync (OneDrive, Dropbox syncing large files)\n- Chrome/Edge with many extensions\n- Video calls (expected: ~15% per hour)\n- Outdated drivers\n- Stuck Windows Update\n\n### When to Request Replacement\n- Design capacity vs full charge < 60%\n- Battery less than 3 hours at normal use\n- Battery swelling (stop using immediately!)\n- Submit IT ticket with battery report attached",
                Category = TicketCategory.Hardware, Tags = ["battery", "laptop", "power", "optimization"],
                ViewCount = 223, HelpfulCount = 189
            },
            new()
            {
                Id = "kb-015", Title = "Azure DevOps Access and Permissions",
                Content = "## Azure DevOps Access Guide\n\n### Access Levels\n- **Basic:** Code repos, pipelines, boards, test plans\n- **Stakeholder:** View boards and backlogs (free)\n- **Visual Studio Subscriber:** Full access with VS license\n\n### Requesting Access\n1. Submit IT ticket with:\n   - Project name\n   - Required access level (Basic recommended)\n   - Permission level: Reader, Contributor, or Project Admin\n   - Manager approval\n\n### Self-Service (if you have access)\n- Go to Project Settings > Permissions\n- Add users to appropriate groups\n- Team-level access can be managed by Team Admins\n\n### Common Permission Groups\n- **Contributors:** Push code, create PRs, manage work items\n- **Readers:** View only, for stakeholders\n- **Project Admins:** Full project configuration\n- **Build Admins:** Manage pipelines only\n\n### Timeline\n- Existing project access: 1 business day\n- New project creation: 2-3 business days",
                Category = TicketCategory.AccessPermissions, Tags = ["azure-devops", "access", "permissions", "git"],
                ViewCount = 312, HelpfulCount = 278
            },
        ];
    }

    private static List<AuditEntry> GetSeedAuditEntries()
    {
        var baseDate = DateTime.UtcNow.AddDays(-14);
        return
        [
            new() { Id = "aud-001", TicketId = "tkt-001", Action = "Created", Actor = "Sarah Chen", Details = "Ticket created via email", Timestamp = baseDate.AddHours(2) },
            new() { Id = "aud-002", TicketId = "tkt-001", Action = "Assigned", Actor = "System", Details = "Auto-assigned to Mike Torres based on category", NewValue = "Mike Torres", Timestamp = baseDate.AddHours(2).AddMinutes(5) },
            new() { Id = "aud-003", TicketId = "tkt-001", Action = "Status Changed", Actor = "Mike Torres", Details = "Began investigation", PreviousValue = "New", NewValue = "Open", Timestamp = baseDate.AddHours(3) },
            new() { Id = "aud-004", TicketId = "tkt-002", Action = "Created", Actor = "James Wilson", Details = "Ticket created via portal", Timestamp = baseDate.AddHours(5) },
            new() { Id = "aud-005", TicketId = "tkt-002", Action = "AI Triage", Actor = "AI System", Details = "Classified as Account Management, Medium priority. Suggested KB article: Password Reset Procedure", Timestamp = baseDate.AddHours(5).AddMinutes(1) },
            new() { Id = "aud-006", TicketId = "tkt-002", Action = "Assigned", Actor = "System", Details = "Assigned to Lisa Park", NewValue = "Lisa Park", Timestamp = baseDate.AddHours(5).AddMinutes(2) },
            new() { Id = "aud-007", TicketId = "tkt-002", Action = "Status Changed", Actor = "Lisa Park", Details = "Password reset initiated", PreviousValue = "Open", NewValue = "InProgress", Timestamp = baseDate.AddHours(6) },
            new() { Id = "aud-008", TicketId = "tkt-007", Action = "Created", Actor = "Nina Patel", Details = "Ticket created - possible phishing report", Timestamp = baseDate.AddDays(3) },
            new() { Id = "aud-009", TicketId = "tkt-007", Action = "Priority Escalated", Actor = "Security Team", Details = "Elevated to Critical - confirmed phishing campaign", PreviousValue = "High", NewValue = "Critical", Timestamp = baseDate.AddDays(3).AddMinutes(30) },
            new() { Id = "aud-010", TicketId = "tkt-014", Action = "Created", Actor = "Kevin Nguyen", Details = "Software installation request", Timestamp = baseDate.AddDays(6).AddHours(5) },
            new() { Id = "aud-011", TicketId = "tkt-014", Action = "Resolved", Actor = "Tom Richards", Details = "Docker Desktop installed and configured. User verified working.", PreviousValue = "InProgress", NewValue = "Resolved", Timestamp = baseDate.AddDays(7) },
            new() { Id = "aud-012", TicketId = "tkt-019", Action = "Created", Actor = "Systems Alert", Details = "Auto-generated from Datadog monitoring alert", Timestamp = baseDate.AddDays(10) },
            new() { Id = "aud-013", TicketId = "tkt-019", Action = "Priority Set", Actor = "AI System", Details = "Critical priority - production payment system affected", NewValue = "Critical", Timestamp = baseDate.AddDays(10).AddMinutes(1) },
            new() { Id = "aud-014", TicketId = "tkt-021", Action = "Resolved", Actor = "Tom Richards", Details = "Rolled back Office update and applied compatibility patch for VBA macros", PreviousValue = "InProgress", NewValue = "Resolved", Timestamp = baseDate.AddDays(12) },
        ];
    }
}
