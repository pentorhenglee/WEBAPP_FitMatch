-- Demo data for FitMatch. Every person here is fictional.
-- Demo login: demo@fitmatch.app / FitMatch2026!  (all seeded users share this password)
-- Event times use now() so upcoming posts stay upcoming whenever you re-seed.

TRUNCATE "Comment", "Member", "Post", "Mission", "Notification", "History", "User" RESTART IDENTITY CASCADE;

INSERT INTO "User" ("Username", "Password_hash", "Email", "ProfileUrl", "Info") VALUES
 ('demo',        '$2a$11$SznM7nVaZr49nIu81TPbae2ZNE/76c3M.8hU.CQc/XSdxOgwdLLCi', 'demo@fitmatch.app',  NULL, 'Demo account. Evening runner, weekend footballer.'),
 ('nattapong_r', '$2a$11$SznM7nVaZr49nIu81TPbae2ZNE/76c3M.8hU.CQc/XSdxOgwdLLCi', 'nattapong@example.com', NULL, 'Trying to get back to 5K under 25 min.'),
 ('ploy.bb',     '$2a$11$SznM7nVaZr49nIu81TPbae2ZNE/76c3M.8hU.CQc/XSdxOgwdLLCi', 'ploy@example.com',   NULL, 'Volleyball every Tuesday. Beginners welcome.'),
 ('tanakrit',    '$2a$11$SznM7nVaZr49nIu81TPbae2ZNE/76c3M.8hU.CQc/XSdxOgwdLLCi', 'tanakrit@example.com', NULL, 'Road bike, Sunday long rides.'),
 ('mint_hoops',  '$2a$11$SznM7nVaZr49nIu81TPbae2ZNE/76c3M.8hU.CQc/XSdxOgwdLLCi', 'mint@example.com',   NULL, '3x3 basketball, looking for a regular team.'),
 ('jirayu.fc',   '$2a$11$SznM7nVaZr49nIu81TPbae2ZNE/76c3M.8hU.CQc/XSdxOgwdLLCi', 'jirayu@example.com', NULL, 'Organises 7-a-side on weekends.');

-- Posts 1-7 upcoming, 8-10 past (feed the dashboard history, stats and top partners)
INSERT INTO "Post" ("UserId", "Title", "Location", "EventDateTime", "Description", "SportType", "CreateDate", "MaxPeople", "ImageUrl", "Status", "Lat", "Lon") VALUES
 (2, 'Evening 5K easy pace',        'Suan Luang Rama IX',        date_trunc('day', now()) + interval '2 days 18 hours',  'Easy 6:30/km pace, no one gets dropped. Meet at the main gate.', 'Running',    now() - interval '1 day',  8,  NULL, 'open', 13.6868, 100.6636),
 (6, 'Sunday 7-a-side football',    'KMITL football field',      date_trunc('day', now()) + interval '4 days 16 hours',  'Need 4 more players. Bring a dark and a light shirt.',            'Soccer',     now() - interval '2 days', 14, NULL, 'open', 13.7297, 100.7766),
 (3, 'Beginner volleyball night',   'KMITL indoor gym',          date_trunc('day', now()) + interval '3 days 19 hours',  'Casual games, we rotate teams every set. First timers OK.',       'Volleyball', now() - interval '1 day',  12, NULL, 'open', 13.7285, 100.7780),
 (4, 'Bang Krachao morning ride',   'Bang Krachao, Phra Pradaeng', date_trunc('day', now()) + interval '6 days 6 hours', '35 km loop, avg 22 km/h, coffee stop halfway.',                  'Cycling',    now() - interval '3 days', 10, NULL, 'open', 13.6930, 100.5620),
 (5, '3x3 pickup basketball',       'Lumphini Park courts',      date_trunc('day', now()) + interval '1 day 17 hours',   'Looking for 2 more to make two full teams.',                      'Basketball', now() - interval '5 hours', 6, NULL, 'open', 13.7307, 100.5418),
 (1, 'Interval session 8x400m',     'KMITL running track',       date_trunc('day', now()) + interval '5 days 18 hours',  'Track intervals, 90s rest. Bring water.',                          'Running',    now() - interval '6 hours', 6, NULL, 'open', 13.7290, 100.7755),
 (6, 'Futsal after class',          'Lat Krabang futsal arena',  date_trunc('day', now()) + interval '8 days 19 hours',  'Court is booked and paid, split 60 baht each.',                    'Soccer',     now() - interval '1 day', 10, NULL, 'open', 13.7240, 100.7590),
 (2, 'Long run 12K',                'Benjakitti Forest Park',    now() - interval '6 days',           'Steady 12K, finished at the skywalk.',                             'Running',    now() - interval '10 days', 8, NULL, 'close', 13.7303, 100.5590),
 (3, 'Volleyball scrimmage',        'KMITL indoor gym',          now() - interval '12 days',          'Mixed teams, great turnout.',                                      'Volleyball', now() - interval '15 days', 12, NULL, 'close', 13.7285, 100.7780),
 (6, 'Weekend football',            'KMITL football field',      now() - interval '20 days',          '7-a-side, two halves of 25 min.',                                  'Soccer',     now() - interval '24 days', 14, NULL, 'close', 13.7297, 100.7766);

-- Every post owner is also a member with status 'owner' (same as PostAPIController.CreatePost)
INSERT INTO "Member" ("PostId", "UserId", "Status", "JoinedAt")
SELECT "PostId", "UserId", 'owner', "CreateDate" FROM "Post";

INSERT INTO "Member" ("PostId", "UserId", "Status", "JoinedAt") VALUES
 (1, 1, 'pending', now() - interval '20 hours'), (1, 4, 'pending', now() - interval '10 hours'),
 (2, 1, 'pending', now() - interval '1 day'),    (2, 2, 'pending', now() - interval '1 day'), (2, 5, 'pending', now() - interval '12 hours'),
 (3, 5, 'pending', now() - interval '8 hours'),
 (5, 1, 'pending', now() - interval '3 hours'),  (5, 6, 'pending', now() - interval '2 hours'),
 (6, 2, 'pending', now() - interval '4 hours'),
 (8, 1, 'pending', now() - interval '9 days'),   (8, 3, 'pending', now() - interval '9 days'),
 (9, 1, 'pending', now() - interval '14 days'),  (9, 2, 'pending', now() - interval '14 days'), (9, 5, 'pending', now() - interval '13 days'),
 (10, 1, 'pending', now() - interval '22 days'), (10, 2, 'pending', now() - interval '22 days'), (10, 5, 'pending', now() - interval '21 days');

INSERT INTO "Comment" ("UserId", "PostId", "CreatedAt", "Text") VALUES
 (1, 1, now() - interval '19 hours', 'Count me in, is 6:30 pace flexible?'),
 (2, 1, now() - interval '18 hours', 'Yes, we wait for everyone at each km.'),
 (5, 2, now() - interval '11 hours', 'Can play goalkeeper if needed.'),
 (6, 2, now() - interval '10 hours', 'Perfect, we were short one.'),
 (6, 5, now() - interval '1 hour',   'On my way after work, around 18:00.'),
 (3, 8, now() - interval '6 days',   'Nice run everyone!');

INSERT INTO "Mission" ("UserId", "Description", "IsCompleted", "CreatedAt") VALUES
 (1, 'Run 20 km this week',          false, now() - interval '2 days'),
 (1, 'Join 2 new activities',        true,  now() - interval '5 days'),
 (1, 'Try a sport I have never played', false, now() - interval '1 day');

INSERT INTO "Notification" ("UserId", "TriggerId", "PostId", "Type", "Message", "IsRead", "CreatedAt") VALUES
 (1, 1, 5, 'Join', 'You have joined the activity ''3x3 pickup basketball''', false, now() - interval '3 hours'),
 (1, 2, 6, 'Join', 'Someone has joined your activity ''Interval session 8x400m''', false, now() - interval '4 hours'),
 (1, 2, 8, 'Close', 'Congratulations! The activity ''Long run 12K'' has been closed. See you there!', true, now() - interval '7 days');
