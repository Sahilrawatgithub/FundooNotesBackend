# 📝 Fundoo Notes Project

## 📌 Project Overview

**Fundoo Notes** is a full-stack web application inspired by Google Keep. It allows users to register, log in, and manage their notes efficiently. This project includes features like password reset via email, OTP-based user authentication, and JWT-based secure login.

The project follows a **Clean Architecture** pattern with separate layers for **Business** and **Repository** operations. It also integrates **NLog** for structured logging.

---

## 🚀 Features

- ✅ **User Registration** – New users can register with the application.
- 🔐 **Login** – Users log in using credentials and receive a JWT token for secure access.
- 🔁 **Forgot Password** – Users can request an OTP to reset their password via email.
- 🔄 **Reset Password** – Users can update their password after OTP verification or while logged in.
- ✉️ **Email Notifications** – OTPs are sent to registered email addresses for password resets.
- 📨 **RabbitMQ Integration** – RabbitMQ is used for efficient background OTP delivery and message queuing.

---

## 🧰 Tech Stack

- **Backend**: ASP.NET Core Web API  
- **Database**: SQL Server  
- **Message Queue**: RabbitMQ  
- **Logging**: NLog  
- **Authentication**: JWT (JSON Web Tokens)
