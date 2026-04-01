-- KOU (UniversityId=142) faculty/department sanity
select count(*) as faculties
from "Faculties"
where "UniversityId" = 142;

select count(*) as professors
from "Professors" p
join "Departments" d on d."Id" = p."DepartmentId"
join "Faculties" f on f."Id" = d."FacultyId"
where f."UniversityId" = 142;

select count(*) as departments
from "Departments" d
join "Faculties" f on f."Id" = d."FacultyId"
where f."UniversityId" = 142;

select
  f."Name" as faculty,
  count(d."Id") as dept_count
from "Faculties" f
left join "Departments" d on d."FacultyId" = f."Id"
where f."UniversityId" = 142
group by f."Name"
order by dept_count desc, f."Name"
limit 20;

