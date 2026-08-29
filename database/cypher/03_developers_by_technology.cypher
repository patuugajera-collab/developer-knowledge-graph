// Question: which developers have experience with a particular technology?
// This is a simple 2-node hop, but it reads the relationship property (proficiency)
// which a relational join table would also carry.

MATCH (d:Developer)-[r:HAS_SKILL]->(t:Technology {name: $technologyName})
RETURN d.name AS developer,
       d.role AS role,
       t.name AS technology,
       r.proficiency AS proficiency,
       r.since AS since
ORDER BY r.proficiency, d.name;