using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Rezqa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets a test message
    /// </summary>
    /// <returns>A test message</returns>
    /// 
 
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
     
        return Ok("Test endpoint is working!");
    }

    /// <summary>
    /// Gets a test message with a parameter
    /// </summary>
    /// <param name="id">The test ID</param>
    /// <returns>A test message with the ID</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetById(int id)
    {
        if (id <= 0)
            return BadRequest("ID must be greater than 0");

        return Ok($"Test endpoint with ID {id} is working!");
    }

    /// <summary>
    /// Creates a test message
    /// </summary>
    /// <param name="message">The message to create</param>
    /// <returns>The created message</returns>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return BadRequest("Message cannot be empty");

        // This will test our XSS protection
        return CreatedAtAction(nameof(GetById), new { id = 1 }, $"Created message: {message}");
    }

    /// <summary>
    /// Updates a test message
    /// </summary>
    /// <param name="id">The ID of the message to update</param>
    /// <param name="message">The new message</param>
    /// <returns>The updated message</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] string message)
    {
        if (id <= 0)
            return BadRequest("ID must be greater than 0");

        if (string.IsNullOrWhiteSpace(message))
            return BadRequest("Message cannot be empty");

        // Simulate not found
        if (id > 100)
            return NotFound($"Message with ID {id} not found");

        return Ok($"Updated message {id}: {message}");
    }

    /// <summary>
    /// Deletes a test message
    /// </summary>
    /// <param name="id">The ID of the message to delete</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        if (id <= 0)
            return BadRequest("ID must be greater than 0");

        // Simulate not found
        if (id > 100)
            return NotFound($"Message with ID {id} not found");

        return NoContent();
    }

    /// <summary>
    /// Tests rate limiting
    /// </summary>
    /// <returns>A message indicating the rate limit test</returns>
    [HttpGet("rate-limit-test")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult RateLimitTest()
    {
        return Ok("Rate limit test endpoint. Try hitting this endpoint multiple times quickly to test rate limiting.");
    }

    /// <summary>
    /// Tests XSS protection
    /// </summary>
    /// <param name="input">The input to test XSS protection</param>
    /// <returns>The sanitized input</returns>
    [HttpPost("xss-test")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult XssTest([FromBody] string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return BadRequest("Input cannot be empty");

        // The input will be automatically sanitized by our XSS middleware
        return Ok($"Sanitized input: {input}");
    }
} 