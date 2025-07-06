# Gaming platform

The Gaming Platform web application is being developed as part of the university's work experience. The aim of the project is to create a web application that will allow users to find information about games by category and download them.

## Project Description

The project is a web application that allows users to browse a catalogue of games, search for them by name, filter by category, and download game files from individual pages.

### Functionalities for users (without registration)

* View the main page with a list of games.
* Search for games by name.
* Filtering/viewing games by specific categories.
* Go to the individual game page for more information.
* Download game files from the individual game page.

### Non-Functional Requirements

* **Interface**: Simple, intuitive and user-friendly.
* **Adaptability**: Correct display on different devices (desktops, tablets, smartphones).
* **Performance**: Fast page loading and responsive interface.
* **Reliability**: Stable operation of the main functionality.

## System architecture

The system consists of client and server parts that interact with the database and external services.

### Technology Stack

* **Backend**: ASP.NET Core Web API (C#).
* **Frontend**: React (JavaScript/TypeScript), CSS3.
* **Database**: MySQL.

### Components

* **Frontend**: A single-page application (SPA) in React, responsible for the user interface and interaction, interacts with the backend via RESTful API.
* **Backend**: ASP.NET Core Web API, provides RESTful services for the frontend, implements business logic, database access.
* **Database:** MySQL for storing information about games. Main entities: `Games` (ID, Title, Description, Developer, Release date, Category, Rating, Download link, System requirements) and `MediaFiles` (Game ID, Banner, Icon, 1st media file, etc.).
* **Image storage**: Cloudinary.
* **Game file storage:** Google Drive (or other hosts), direct links are saved.

* **Scheme**:
![image](https://github.com/user-attachments/assets/e9c22b37-8de4-41ca-89bc-022a1314201a)


## Deployment and Hosting

**Frontend**: Vercel.
* **Backend**: Render.
* **Database**: Aiven.io.
* **Code**: GitHub.

## Project Development and Management.

* **Development Environment**:
    * Backend: Microsoft Visual Studio 2022.
    * Frontend: Visual Studio Code (or similar).
    * Database: MySQL.
* **Version control system**: GitHub.
* **Task management system**: Jira.

## Team and Responsibilities

The project is developed by a team of two people:

* **Developer 1 (Backend Focus)**: Database design, ASP.NET Core Web API development, setting up interaction with the database, providing endpoint APIs, basic backend testing.
* **Developer 2 (Frontend Focus)**: React UI development, API integration, client logic implementation, basic frontend testing.

**General tasks**: Planning, API coordination, joint testing, documentation, communication.

## Notes.

* The database is filled with content (games, descriptions, images, links) by developers manually or using scripts.
* Security issues (data validation, XSS protection, CSRF) are important for future extensions, but at the current stage (training project), the main focus is on functionality.
