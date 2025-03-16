workspace "Shabnameh" "AI based newsletter system" {
!identifiers hierarchical
    model {
        shabnameh = softwareSystem "Shabnameh" {
            description "Collects news, summarizes with Ollama, sends newsletters"
        }
        
        sources = softwareSystem "News Websites" {
            description "Various news sources"
            tag "external"
        }

        ollama = softwareSystem "Ollama Summarizer" {
            description "LLM for summarizing news"
            tag "external"
        }

        email = softwareSystem "SMTP Server" {
            description "Server for sending emails"
            tag "external"
        }

        subscriber = person "Subscriber" {
            description "User receiving the newsletter"
        }
        
        admin = person "Admin" {
            description "Admin managing configs, subscriber and monitoring system"
        }

        // Relationships
        shabnameh -> sources "Fetches news from"
        shabnameh -> ollama "Requests news summary"
        shabnameh -> email "Sends newsletters through"
        email -> subscriber "Delivers newsletter to"
        subscriber -> shabname "Define topics prefrences"
        admin -> shabname "Manages the system"
    }
    
    views {
        systemLandscape {
            include *
            autolayout lr
        }  

        properties {
            structurizr.sort "type"
        }
        theme default
        
        styles {
            element "Container" {
                background #55aa55
            }
            element "Database" {
                shape cylinder
            }
            element "external" {
                background #8c8496
            }
        }
    }
}