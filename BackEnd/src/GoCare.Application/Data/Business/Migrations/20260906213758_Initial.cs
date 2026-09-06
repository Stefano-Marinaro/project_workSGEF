using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Application.Data.Business.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "associations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    headquarter_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    headquarter_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    headquarter_postal_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    headquarter_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    headquarter_province = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    phones = table.Column<List<string>>(type: "text[]", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    availability_hours = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    covered_provinces = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_associations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_type = table.Column<string>(type: "text", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deactivated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_type = table.Column<string>(type: "text", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    channels = table.Column<int>(type: "integer", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    home_address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    home_address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    home_address_postal_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    home_address_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    home_address_province = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    anonymized_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "care_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_care_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_care_groups_persons_created_by",
                        column: x => x.created_by,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "saved_destinations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    saved_address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    saved_address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    saved_address_postal_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    saved_address_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    saved_address_province = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saved_destinations", x => x.id);
                    table.ForeignKey(
                        name: "fk_saved_destinations_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "care_group_memberships",
                columns: table => new
                {
                    care_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_role = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    invitation_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    invitation_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    responded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_care_group_memberships", x => new { x.care_group_id, x.person_id });
                    table.ForeignKey(
                        name: "fk_care_group_memberships_care_groups_care_group_id",
                        column: x => x.care_group_id,
                        principalTable: "care_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_group_memberships_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transport_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    beneficiary_id = table.Column<Guid>(type: "uuid", nullable: false),
                    care_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trip_type = table.Column<string>(type: "text", nullable: false),
                    trip_direction = table.Column<string>(type: "text", nullable: false),
                    departure_date_hour = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    return_date_hour = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    start_address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    start_address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_address_postal_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    start_address_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    start_address_province = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    end_address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    end_address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    end_address_postal_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    end_address_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    end_address_province = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    return_end_address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    return_end_address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    return_end_address_postal_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    return_end_address_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    return_end_address_province = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    reference_phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reference_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    request_status = table.Column<string>(type: "text", nullable: false),
                    assigned_association_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    not_covered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_transport_requests_associations_assigned_association_id",
                        column: x => x.assigned_association_id,
                        principalTable: "associations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transport_requests_care_groups_care_group_id",
                        column: x => x.care_group_id,
                        principalTable: "care_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transport_requests_persons_beneficiary_id",
                        column: x => x.beneficiary_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transport_requests_persons_requested_by_id",
                        column: x => x.requested_by_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "companions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    surname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    relationship = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companions", x => x.id);
                    table.ForeignKey(
                        name: "fk_companions_transport_requests_transport_request_id",
                        column: x => x.transport_request_id,
                        principalTable: "transport_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contact_access_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    association_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_access_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_contact_access_logs_associations_association_id",
                        column: x => x.association_id,
                        principalTable: "associations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contact_access_logs_transport_requests_transport_request_id",
                        column: x => x.transport_request_id,
                        principalTable: "transport_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transport_modification_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    outcome_message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_modification_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_transport_modification_requests_transport_requests_transpor",
                        column: x => x.transport_request_id,
                        principalTable: "transport_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transport_request_candidates",
                columns: table => new
                {
                    transport_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    association_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_request_candidates", x => new { x.transport_request_id, x.association_id });
                    table.ForeignKey(
                        name: "fk_transport_request_candidates_associations_association_id",
                        column: x => x.association_id,
                        principalTable: "associations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transport_request_candidates_transport_requests_transport_r",
                        column: x => x.transport_request_id,
                        principalTable: "transport_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transport_request_rejections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    association_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_request_rejections", x => x.id);
                    table.ForeignKey(
                        name: "fk_transport_request_rejections_associations_association_id",
                        column: x => x.association_id,
                        principalTable: "associations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transport_request_rejections_transport_requests_transport_r",
                        column: x => x.transport_request_id,
                        principalTable: "transport_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trip_status_transitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    made_by_association_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operator_label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_status_transitions", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_status_transitions_associations_made_by_association_id",
                        column: x => x.made_by_association_id,
                        principalTable: "associations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trip_status_transitions_transport_requests_transport_reques",
                        column: x => x.transport_request_id,
                        principalTable: "transport_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_care_group_memberships_invitation_token",
                table: "care_group_memberships",
                column: "invitation_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_care_group_memberships_person_id",
                table: "care_group_memberships",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_groups_created_by_name",
                table: "care_groups",
                columns: new[] { "created_by", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_companions_transport_request_id",
                table: "companions",
                column: "transport_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_access_logs_association_id",
                table: "contact_access_logs",
                column: "association_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_access_logs_transport_request_id",
                table: "contact_access_logs",
                column: "transport_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_tokens_subject_type_subject_id",
                table: "device_tokens",
                columns: new[] { "subject_type", "subject_id" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_subject_type_subject_id",
                table: "notifications",
                columns: new[] { "subject_type", "subject_id" });

            migrationBuilder.CreateIndex(
                name: "ix_saved_destinations_person_id",
                table: "saved_destinations",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_modification_requests_transport_request_id",
                table: "transport_modification_requests",
                column: "transport_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_request_candidates_association_id",
                table: "transport_request_candidates",
                column: "association_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_request_rejections_association_id",
                table: "transport_request_rejections",
                column: "association_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_request_rejections_transport_request_id",
                table: "transport_request_rejections",
                column: "transport_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_requests_assigned_association_id",
                table: "transport_requests",
                column: "assigned_association_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_requests_beneficiary_id",
                table: "transport_requests",
                column: "beneficiary_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_requests_care_group_id",
                table: "transport_requests",
                column: "care_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_requests_request_status",
                table: "transport_requests",
                column: "request_status");

            migrationBuilder.CreateIndex(
                name: "ix_transport_requests_requested_by_id",
                table: "transport_requests",
                column: "requested_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_requests_start_address_province",
                table: "transport_requests",
                column: "start_address_province");

            migrationBuilder.CreateIndex(
                name: "ix_trip_status_transitions_made_by_association_id",
                table: "trip_status_transitions",
                column: "made_by_association_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_status_transitions_transport_request_id_occurred_at",
                table: "trip_status_transitions",
                columns: new[] { "transport_request_id", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "care_group_memberships");

            migrationBuilder.DropTable(
                name: "companions");

            migrationBuilder.DropTable(
                name: "contact_access_logs");

            migrationBuilder.DropTable(
                name: "device_tokens");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "saved_destinations");

            migrationBuilder.DropTable(
                name: "transport_modification_requests");

            migrationBuilder.DropTable(
                name: "transport_request_candidates");

            migrationBuilder.DropTable(
                name: "transport_request_rejections");

            migrationBuilder.DropTable(
                name: "trip_status_transitions");

            migrationBuilder.DropTable(
                name: "transport_requests");

            migrationBuilder.DropTable(
                name: "associations");

            migrationBuilder.DropTable(
                name: "care_groups");

            migrationBuilder.DropTable(
                name: "persons");
        }
    }
}
