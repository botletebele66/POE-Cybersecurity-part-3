using System;
using Cybrog.Engine;
using Cybrog.Models;
using Xunit;

namespace Cybrog.Tests
{
    public class QuizEngineTests
    {
        [Fact]
        public void TotalQuestions_IsAtLeastTen()
        {
            var quiz = new QuizEngine();
            Assert.True(quiz.TotalQuestions >= 10, "Rubric requires at least 10 quiz questions.");
        }

        [Fact]
        public void QuestionBank_ContainsBothMultipleChoiceAndTrueFalse()
        {
            var quiz = new QuizEngine();
            quiz.Start();
            bool sawMcq = false, sawTf = false;
            for (int i = 0; i < quiz.TotalQuestions; i++)
            {
                var q = quiz.GetNextQuestion();
                Assert.NotNull(q);
                if (q!.Type == QuestionType.MultipleChoice) sawMcq = true;
                if (q.Type == QuestionType.TrueFalse) sawTf = true;
            }
            Assert.True(sawMcq);
            Assert.True(sawTf);
        }

        [Fact]
        public void Start_ResetsScoreAndQuestionNumber()
        {
            var quiz = new QuizEngine();
            quiz.Start();
            quiz.GetNextQuestion();
            quiz.SubmitAnswer(0); // may or may not be correct, irrelevant here

            quiz.Start(); // restart
            Assert.Equal(0, quiz.Score);
            Assert.Equal(0, quiz.CurrentQuestionNumber);
            Assert.True(quiz.IsActive);
        }

        [Fact]
        public void SubmitAnswer_CorrectIndex_IncrementsScore()
        {
            var quiz = new QuizEngine();
            quiz.Start();
            var q = quiz.GetNextQuestion();
            Assert.NotNull(q);

            bool result = quiz.SubmitAnswer(q!.CorrectIndex);

            Assert.True(result);
            Assert.Equal(1, quiz.Score);
        }

        [Fact]
        public void SubmitAnswer_IncorrectIndex_DoesNotIncrementScore()
        {
            var quiz = new QuizEngine();
            quiz.Start();
            var q = quiz.GetNextQuestion();
            Assert.NotNull(q);
            int wrongIndex = q!.CorrectIndex == 0 ? 1 : 0;

            bool result = quiz.SubmitAnswer(wrongIndex);

            Assert.False(result);
            Assert.Equal(0, quiz.Score);
        }

        [Fact]
        public void SubmitAnswer_WithoutActiveQuestion_Throws()
        {
            var quiz = new QuizEngine();
            // Never started — no current question.
            Assert.Throws<InvalidOperationException>(() => quiz.SubmitAnswer(0));
        }

        [Fact]
        public void GetNextQuestion_AfterAllQuestionsExhausted_ReturnsNullAndDeactivates()
        {
            var quiz = new QuizEngine();
            quiz.Start();
            for (int i = 0; i < quiz.TotalQuestions; i++)
            {
                var q = quiz.GetNextQuestion();
                Assert.NotNull(q);
                quiz.SubmitAnswer(q!.CorrectIndex);
            }

            var afterLast = quiz.GetNextQuestion();

            Assert.Null(afterLast);
            Assert.False(quiz.IsActive);
            Assert.Null(quiz.CurrentQuestion);
        }

        [Fact]
        public void GetFinalResult_PerfectScore_ReturnsTopTierMessage()
        {
            var quiz = new QuizEngine();
            quiz.Start();
            for (int i = 0; i < quiz.TotalQuestions; i++)
            {
                var q = quiz.GetNextQuestion()!;
                quiz.SubmitAnswer(q.CorrectIndex);
            }

            var (score, total, message) = quiz.GetFinalResult();

            Assert.Equal(total, score);
            Assert.Contains("pro", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void EachQuestion_HasCorrectIndexWithinOptionsRange()
        {
            // Guards against a data-entry mistake in the question bank (e.g. CorrectIndex
            // pointing past the end of the Options array, which would silently corrupt scoring).
            var quiz = new QuizEngine();
            quiz.Start();
            for (int i = 0; i < quiz.TotalQuestions; i++)
            {
                var q = quiz.GetNextQuestion()!;
                Assert.InRange(q.CorrectIndex, 0, q.Options.Count - 1);
                Assert.False(string.IsNullOrWhiteSpace(q.Explanation));
                Assert.False(string.IsNullOrWhiteSpace(q.Category));
            }
        }

        [Fact]
        public void Start_ShufflesQuestionOrder_AcrossMultipleRuns()
        {
            // Not a strict guarantee (shuffle could coincidentally match), but with 15
            // questions the probability of an identical order twice in a row is negligible,
            // so this is a reasonable smoke test that shuffling is actually happening.
            var quiz = new QuizEngine();

            quiz.Start();
            var firstOrder = new System.Collections.Generic.List<string>();
            for (int i = 0; i < quiz.TotalQuestions; i++)
                firstOrder.Add(quiz.GetNextQuestion()!.Text);

            quiz.Start();
            var secondOrder = new System.Collections.Generic.List<string>();
            for (int i = 0; i < quiz.TotalQuestions; i++)
                secondOrder.Add(quiz.GetNextQuestion()!.Text);

            Assert.NotEqual(firstOrder, secondOrder);
        }

        [Fact]
        public void Reset_ClearsAllState()
        {
            var quiz = new QuizEngine();
            quiz.Start();
            var q = quiz.GetNextQuestion()!;
            quiz.SubmitAnswer(q.CorrectIndex);

            quiz.Reset();

            Assert.False(quiz.IsActive);
            Assert.Equal(0, quiz.Score);
            Assert.Equal(0, quiz.CurrentQuestionNumber);
            Assert.Null(quiz.CurrentQuestion);
        }
    }
}
