# RCA Projekat - Razvoj Cloud Aplikacija - Reddit Forum

## Project Description

The **Reddit** project is an **Azure Cloud Service** solution developed in Visual Studio. It consists of multiple components that handle different functionalities, including email notifications, health monitoring, and core forum operations. Below is a brief description of each component:

### Components

- **AdminsEmailConfigurator**: A console application that allows administrators to input their email addresses for receiving system notifications.

- **Common**: A shared project containing repositories for Azure Cloud Tables, Queues, and Blobs. It also includes interfaces used in WCF communication as contracts, models, and methods for sending emails.

- **HealthMonitoringWorkerRole**: A worker role that checks every 5 seconds whether the email services and the Reddit web services are running properly.

- **HealthStatusWebRole**: A web role providing a visual application for monitoring the status of services such as Reddit and Notification services.

- **NotificationWorkerRole**: A worker role that reads from the Azure Cloud Queue and sends notifications to post owners when their posts are commented on. It also sends email alerts to administrators when any service fails.

- **RedditWebRole**: The core web role responsible for the main functionality of the Reddit-like forum system. It supports post creation, commenting, upvoting, downvoting, user login, registration, and profile management.

This architecture ensures a reliable and scalable forum application with real-time monitoring and notifications.

## Architecture

![Reddit](https://github.com/user-attachments/assets/9aaee978-e07a-4c9a-804e-5139808a5c14)

## Participants

- **Filip Bogdanović PR126/2020** - [Cofi21]
- **Luka Vidaković PR137/2020** - [LukaVidakovic]
- **Nemanja Mijonić PR138/2020** - [nemanjamijonic]
- **Srdjan Bogićević PR139/2020** - [blackhood10]
