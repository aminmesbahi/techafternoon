workspace "Shabnameh" "AI based newsletter system" {
!identifiers hierarchical
    model {
        shabnameh = softwareSystem "Shabnameh" {
            description "Collects news, summarizes with Ollama, sends newsletters"

            webApp = container "Web Application" {
                description "Allows subscribers to manage preferences, admins to configure the system"
                technology "ASP.NET Core MVC"

                subscriberController = component "Subscriber Controller" {
                    description "Manages subscriber preferences"
                    technology "ASP.NET Controller"
                }

                adminController = component "Admin Controller" {
                    description "Manages admin operations"
                    technology "ASP.NET Controller"
                }
            }

            newsCollector = container "News Collector" {
                description "Fetches news from websites periodically"
                technology "Worker Service"

                scraperComponent = component "Scraper Component" {
                    description "Extracts news articles from websites"
                    technology "Scrapy"
                }
            }

            summarizer = container "Summarizer" {
                description "Summarizes news articles using Ollama"
                technology "REST Client"

                ollamaClient = component "Ollama Client" {
                    description "Sends summary requests to Ollama API"
                    technology "HTTP Client"
                }
            }

            newsletterEngine = container "Newsletter Engine" {
                description "Creates daily newsletters and schedules email sending"
                technology "Quartz.NET Scheduler"

                composer = component "Newsletter Composer" {
                    description "Composes summarized articles into newsletters"
                    technology "HTML Template Engine"
                }

                emailSender = component "Email Sender" {
                    description "Sends newsletters via SMTP"
                    technology "SMTP Client"
                }
            }

            database = container "Database" {
                description "Stores subscribers, preferences, schedules, and collected news"
                technology "SQL Server"
            }
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
            description "Admin managing configs, subscribers, and monitoring system"
        }

        // Relationships
        subscriber -> shabnameh.webApp "Manages preferences via"
        admin -> shabnameh.webApp "Configures system via"

        shabnameh.webApp -> shabnameh.database "Reads/Writes subscriber & config data"
        shabnameh.newsCollector -> sources "Fetches news from"
        shabnameh.newsCollector -> shabnameh.database "Stores news articles in"

        shabnameh.summarizer -> shabnameh.database "Reads articles from"
        shabnameh.summarizer -> ollama "Requests summaries from"
        shabnameh.summarizer -> shabnameh.database "Stores summaries in"

        shabnameh.newsletterEngine -> shabnameh.database "Reads summaries & preferences from"
        shabnameh.newsletterEngine -> email "Sends newsletters through"

        email -> subscriber "Delivers newsletter to"
    }

    views {
        systemContext shabnameh "Diagram1" {
            include *
            autolayout lr
        }

container shabnameh "Diagram2" {
            include shabnameh.webApp shabnameh.newsCollector shabnameh.summarizer shabnameh.newsletterEngine shabnameh.database sources ollama email subscriber admin
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
