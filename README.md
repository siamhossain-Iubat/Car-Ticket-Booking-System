# Car Ticket Booking System

A web-based Car Ticket Booking System developed using ASP.NET Core MVC to help counter staff manage passenger bookings, seat availability, ticket generation, and booking cancellations.

## Project Overview

The Car Ticket Booking System is designed exclusively for counter staff to simplify and manage daily ticket booking operations.

Counter staff can select travel routes and dates, check available seats, enter passenger details, confirm bookings, and generate unique ticket IDs. The system stores booking information in a SQL Server database to maintain records even after refreshing the application.

It also supports booking cancellation using a ticket number. Once a booking is cancelled, the corresponding seat becomes available for rebooking.

The system includes SMS notification integration using the SMS.net.bd API to send booking confirmation messages to passengers, subject to API configuration and provider restrictions.

## User Role & Responsibilities

The application is designed for a single operational role: **Counter Staff**.

| Role | Main Responsibilities |
|---|---|
| **Counter Staff** | Manage daily bookings, select routes and travel dates, check seat availability, enter passenger details, confirm bookings, generate ticket IDs, view booking records, and cancel bookings using ticket numbers. |

## Main Features

- Counter staff dashboard
- Date-wise booking management
- Travel route selection
- Travel date selection
- Seat availability checking
- Seat selection for passengers
- Prevention of duplicate seat bookings for the same route and travel date
- Passenger information entry
- Automatic unique ticket ID generation
- Booking records stored in SQL Server
- View bookings by travel date
- Booking cancellation using ticket numbers
- Cancelled seats become available for rebooking
- SMS booking confirmation integration using SMS.net.bd API

> SMS delivery depends on valid API configuration, sufficient balance, and provider restrictions.

## Technology Stack

- **Framework:** ASP.NET Core MVC
- **Language:** C#
- **Target Framework:** .NET 8
- **ORM:** Entity Framework Core
- **Database:** Microsoft SQL Server
- **Frontend:** Razor Views, HTML, CSS, JavaScript
- **HTTP Integration:** HttpClient
- **SMS Provider:** SMS.net.bd API
- **IDE:** Microsoft Visual Studio

## System Architecture

```text
              Counter Staff
                   |
                   v
          ASP.NET Core MVC
       Controllers | Models | Views
                   |
                   v
         Entity Framework Core
                   |
                   v
          Microsoft SQL Server
                   |
                   v
             Booking Records

          Booking Confirmation
                   |
                   v
             SMS.net.bd API
                   |
                   v
          Passenger's Mobile Phone
