# CineQuest

[https://github.com/haneefmhmmd/CineQuest/blob/master/Project-Recording.mp4](https://github.com/user-attachments/assets/ff793e37-3fd6-43b9-a5c4-3e7099023907
)

A full-featured movie streaming platform built with ASP.NET Core MVC. This application allows users to sign up, upload movies, and manage movie details. Other users can view movies, add comments, and rate them. The app uses AWS services for efficient storage and management of movie data.

## Features

- **User Authentication**: Secure sign-up and login functionality.
- **Movie Management**: Upload, edit, and manage movies and associated banner images.
- **Rating & Comments**: Signed-up users can view movies, rate them, and add comments.
- **Cloud Storage**: Movies and banner images are stored in AWS S3.
- **Metadata Management**: Movie metadata such as name, rating, genre, director, and release date are stored in DynamoDB.
- **User Information**: User data is securely stored in AWS RDS.
- **Configuration Management**: Configuration settings are stored in AWS Parameter Store for easy management.

## Technologies Used

- **ASP.NET Core MVC**: The backend framework for building the application.
- **AWS S3**: Used for storing movie files and associated banner images.
- **AWS DynamoDB**: Stores movie metadata including ratings, comments, and genre.
- **AWS RDS**: Stores user information securely.
- **AWS Parameter Store**: Used to securely store configuration settings.

## Getting Started

### Prerequisites

To run this project locally, you need to have the following installed:

- [ASP.NET Core SDK](https://dotnet.microsoft.com/download)
- [AWS CLI](https://aws.amazon.com/cli/)
- An active AWS account with access to S3, DynamoDB, RDS, and Parameter Store.

### Setup

1. Clone the repository:

   ````bash
   git clone https://github.com/yourusername/movie-streaming-app.git ```

   ````

2. Navigate to the project directory:
   `bashcd movie-streaming-app`

3.Set up AWS credentials in the AWS CLI:
`bashaws configure`

4. Update configuration details in the appsettings.json file for AWS services, RDS, and S3.

5. Run the application:

`dotnet run`

6. Open the application in your browser:

### Usage

1. **Sign Up**: Create a new account to start uploading movies and manage your profile.
2. **Upload Movies**: Add your movie files and banners. Edit movie details at any time.
3. **View Movies**: Explore movies uploaded by other users, leave ratings, and comments.
4. **Admin Features**: Admin users can manage movie content and user accounts.

## Author

This project was developed by **Haneef Muhammad** and **Deanne Laylay**.
