USE proilic;

INSERT INTO Categories (Name) VALUES ("games");
INSERT INTO Categories (Name) VALUES ("tools");
INSERT INTO Categories (Name) VALUES ("productivity");
INSERT INTO Categories (Name) VALUES ("social");

-- apps: angry birds - game;  weather - tools;  calendar - tools, productivity;  linked-in - social, productivity;
INSERT INTO AppCategories VALUES (1, 1);
INSERT INTO AppCategories VALUES (2, 2);
INSERT INTO AppCategories VALUES (3, 2);
INSERT INTO AppCategories VALUES (3, 3);
INSERT INTO AppCategories VALUES (4, 4);
INSERT INTO AppCategories VALUES (4, 3);

INSERT INTO Reviews (UserId, AppId, Title, Content, Rating, PostDate) VALUES ("seed-user-0001", 1, "Nostalgia", "It reminds me of 2010 and 2011", 5, NOW());
INSERT INTO Reviews (UserId, AppId, Title, Content, Rating, PostDate) VALUES ("seed-user-0002", 1, "Not working", "It doesn't open for me", 1, NOW());
INSERT INTO Reviews (UserId, AppId, Title, Content, Rating, PostDate) VALUES ("seed-user-0002", 3, "Not very convenient", "Other calendar apps are more convienent and user-friendly. I''d love to see the ability to classify and colour code each entry. As someone who tracks personal, social, and work schedules as well as bill payments, family events, etc. all in my calendar app, I''d love to be able to colour code each individual entry without having to create a shared email section rather than just by which email is it associated with", 2, NOW());
INSERT INTO Reviews (UserId, AppId, Title, Content, Rating, PostDate) VALUES ("seed-user-0001", 3, "Fix the alerts!", "I used to love this calendar. it was simple and alerted me to all my appointments but it randomly started sending me multiple alerts for birthdays. there is no way to turn that off and it''s annoying! that should be optional. the only way to disable it is to take every birthday off the calendar, which makes zero sense. fix that asap and I''ll change my opinion. as of now, the app is useless trash with that annoyance!", 1, NOW());
INSERT INTO Reviews (UserId, AppId, Title, Content, Rating, PostDate) VALUES ("seed-user-0001", 2, "It's alright", "some unusual UI features. a novel approach to displaying the forecast. some symbols a bit small. don''t need the news at the bottom of the home page, can''t find out how to get rid of it. I like the incorporation of Met Office weather warnings with mapping. No data on the maps page. it says \"now casting data not available\". Is that because we are not in the USA? I like the monthly display. adverts are the normal scams, using the internet to bypass rules on accuracy and financial probity.", 3, NOW());
INSERT INTO Reviews (UserId, AppId, Title, Content, Rating, PostDate) VALUES ("seed-user-0001", 4, "Great game", "desc", 4, NOW());
INSERT INTO Reviews (UserId, AppId, Title, Content, Rating, PostDate) VALUES ("seed-user-0002", 4, "Great game", "desc", 4, NOW());

