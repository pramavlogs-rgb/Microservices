CREATE TABLE public."Posts" (
    "PostId" SERIAL PRIMARY KEY,
    "UserId" INT,
    "PostTitle" VARCHAR(255),
    "PostContent" TEXT,
    "PostCreated" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "PostUpdated" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

select * from public."Posts"

INSERT INTO public."Posts" ("UserId", "PostTitle", "PostContent")
VALUES (1, 'My First Post', 'This is the content of the post.');